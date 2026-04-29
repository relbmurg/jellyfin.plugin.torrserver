using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TorrServer.Client;
using Jellyfin.Plugin.TorrServer.Configuration;
using Jellyfin.Plugin.TorrServer.Core.Parser;
using Jellyfin.Plugin.TorrServer.Extensions;
using Jellyfin.Plugin.TorrServer.Infrastructure;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TorrServer.Core;

internal sealed class WatchdogService(
    IPluginConfigurationProvider configFactory,
    ITorrServerClientFactory clientFactory,
    ILibraryManager libraryManager,
    ILogger<WatchdogService> logger) : IHostedService, ISyncService, IDisposable
{
    private readonly TimeSpan _grace = TimeSpan.FromSeconds(60);
    private readonly ConcurrentDictionary<string, PendingItem> _removing = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, PendingItem> _blocking = new(StringComparer.OrdinalIgnoreCase);
    private readonly CancellationTokenSource _cts = new();
    private readonly FolderNameParser _folderNameParser = new();
    private readonly FileNameParser _fileNameParser = new();

    private IApiClient Client => clientFactory.GetClient();

    private PluginConfiguration Config => configFactory.Get();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        libraryManager.ItemAdded += LibraryManagerOnItemAdded;
        libraryManager.ItemRemoved += LibraryManagerOnItemRemoved;
        return Task.FromResult(Task.CompletedTask);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        libraryManager.ItemAdded -= LibraryManagerOnItemAdded;
        libraryManager.ItemRemoved -= LibraryManagerOnItemRemoved;
        await _cts.CancelAsync().ConfigureAwait(false);
    }

    private async void LibraryManagerOnItemAdded(object? sender, ItemChangeEventArgs e)
    {
        try
        {
            var kind = e.Item.GetMediaKind();
            if (kind != MediaKind.Movie)
            {
                return;
            }

            var hashes = TorrentHashHelper.GetExistedHashes([e.Item.ContainingFolderPath], false);
            if (hashes.Count == 0)
            {
                return;
            }

            foreach (var hash in hashes)
            {
                if (_removing.TryRemove(hash, out var item))
                {
                    item.Dispose();
                }
            }

            e.Item.SaveItemMetadata(new ItemMetadata(hashes));
            await libraryManager.UpdateItemAsync(e.Item, e.Parent, ItemUpdateType.MetadataEdit, CancellationToken.None)
                .ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            logger.LogError(exception, "Failed to add TorrServer metadata");
        }
    }

    private void LibraryManagerOnItemRemoved(object? sender, ItemChangeEventArgs e)
    {
        try
        {
            using var loggerScope = logger.BeginScope(("Name", e.Item.Name));
            if (!Config.RemoveTorrent || (e.Item.GetMediaKind() is var kind && kind != MediaKind.Movie))
            {
                return;
            }

            var meta = e.Item.GetItemMetadata();
            if (meta == null || meta.Hashes.Count == 0)
            {
                return;
            }

            foreach (var hash in meta.Hashes)
            {
                if (_removing.TryRemove(hash, out var existing))
                {
                    existing.Dispose();
                }

                var item = new PendingItem(hash, kind, _cts.Token);
                if (_removing.TryAdd(hash, item))
                {
                    item.Start(_grace, DeleteTorrent);
                }
                else
                {
                    item.Dispose();
                }
            }
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            logger.LogError(exception, "Failed to remove torrent: {Name}", e.Item.Name);
        }
    }

    public void Dispose()
    {
        libraryManager.ItemAdded -= LibraryManagerOnItemAdded;
        libraryManager.ItemRemoved -= LibraryManagerOnItemRemoved;
        _cts.Dispose();
    }

    public async Task ProcessTorrents(IProgress<double> progress, CancellationToken cancellation)
    {
        logger.LogInformation("Start");
        var config = Config;

        var moviesRoot = libraryManager.GetLibraryPaths(config.MoviesLibraryId).FirstOrDefault(x => x.Equals(config.MoviesLocation, StringComparison.OrdinalIgnoreCase));
        var showsRoot = libraryManager.GetLibraryPaths(config.TvShowsLibraryId).FirstOrDefault(x => x.Equals(config.TvShowsLocation, StringComparison.OrdinalIgnoreCase));

        if (moviesRoot == null && showsRoot == null)
        {
            logger.LogWarning("Library paths are missing. Skipping");
            return;
        }

        var roots = new Dictionary<Category, string?>()
        {
            { Category.Movie, moviesRoot },
            { Category.Tv,  showsRoot },
        };

        var existed = TorrentHashHelper.GetExistedHashes(libraryManager.GetLibrariesPaths());
        var pending = _removing.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var removed = _blocking.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var torrents = await Client.List(cancellation).ConfigureAwait(false);

        var total = torrents.Length;

        var filtered = torrents
            .Select((item, index) =>
            {
                progress.Report((double)index / total * 100);
                return item;
            })
            .Where(x =>
                x.Category is Category.Movie or Category.Tv
                && !existed.Contains(x.Hash)
                && !pending.Contains(x.Hash)
                && !removed.Contains(x.Hash));

        foreach (var item in filtered)
        {
            if (cancellation.IsCancellationRequested)
            {
                break;
            }

            if (!roots.TryGetValue(item.Category, out var root) || string.IsNullOrWhiteSpace(root))
            {
                continue;
            }

            using var scope = logger.BeginScope(new[] { ("Title", item.Title), ("Hash", item.Hash) });
            logger.LogInformation("Start processing");

            var parseResult = _folderNameParser.Parse(item.Title);
            var folder = parseResult.FolderName.ToSafeFileName('.');
            if (item.Category == Category.Tv && FindFolder(root, parseResult, out var exists))
            {
                folder = exists;
            }

            logger.LogInformation("Folder name -> {Folder}", folder);

            var playlist = await Client.GetPlaylist(item.Hash, cancellation).ConfigureAwait(false);
            logger.LogInformation("Playlist received. Items: {ItemsCount}", playlist.Entries.Count);

            if (playlist.Entries.Count == 0)
            {
                logger.LogWarning("Playlist items are missing, skipping.");
                continue;
            }

            await CreateMediaEntry(root, folder, playlist, item.Category, item.Hash, cancellation: cancellation).ConfigureAwait(false);
        }

        progress.Report(100);
        logger.LogInformation("Stop");
    }

    private bool FindFolder(string root, ParseResult parseResult, out string folder)
    {
        var match = Directory.EnumerateDirectories(root, $"{parseResult.Title}*", SearchOption.TopDirectoryOnly)
             .FirstOrDefault(x =>
             {
                 var res = _folderNameParser.Parse(Path.GetFileName(x).Replace('.', ':'));
                 return res.Title.Equals(parseResult.Title, StringComparison.OrdinalIgnoreCase);
             });

        folder = match == null ? string.Empty : Path.GetFileName(match);

        return match != null;
    }

    private async Task CreateMediaEntry(string root, string folder, Playlist playlist, Category category, string hash, string[]? parameters = null, CancellationToken cancellation = default)
    {
        var additionalParameters = string.Join("&", parameters ?? []);
        var directory = Path.Combine(root, folder);
        Directory.CreateDirectory(directory);

        var hashFilename = hash.GetHashFileName();
        if (File.Exists(Path.Combine(directory, hashFilename)))
        {
            logger.LogInformation("Already saved. Skipping");
            return;
        }

        var season = new HashSet<int>();
        var sub = string.Empty;
        foreach (var entry in playlist.Entries)
        {
            var parseResult = _fileNameParser.Parse(entry.Title);
            var file = parseResult.StrmFileName.ToSafeFileName(' ');

            if (category == Category.Tv && parseResult.Season.HasValue && season.Add(parseResult.Season.Value))
            {
                sub = $"Season {parseResult.Season:D2}";
            }

            var path = Path.Combine(directory, sub, file);
            var url = $"{entry.Uri}";
            if (!string.IsNullOrEmpty(additionalParameters))
            {
                url += "&{additionalParameters}";
            }

            var i = 1;
            while (File.Exists(path))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var ext = Path.GetExtension(file);
                path = Path.Combine(directory, sub, $"{name}.{i}{ext}");
                i++;
            }

            Directory.CreateDirectory(Path.Combine(directory, sub));
            await File.WriteAllTextAsync(path, url, cancellation).ConfigureAwait(false);
            logger.LogInformation("Playlist entry saved to {Location}", path);
        }

        // multiseason playlist -> place hash to root
        // otherwise to season folder
        if (season.Count > 1)
        {
            sub = string.Empty;
        }

        File.Create(Path.Combine(directory, sub, hashFilename)).Close();
        logger.LogInformation("Hash file saved");
    }

    private async Task DeleteTorrent(PendingItem item, CancellationToken cancellation)
    {
        if (!_removing.TryRemove(item.Hash, out _))
        {
            return;
        }

        var postRemoved = new PendingItem(item.Hash, item.Kind, _cts.Token);
        if (_blocking.TryAdd(item.Hash, postRemoved))
        {
            var settings = await Client.GetConfiguration(cancellation).ConfigureAwait(false);
            postRemoved.Start(TimeSpan.FromSeconds(settings.TorrentDisconnectTimeout), (deleted, token) =>
            {
                _blocking.TryRemove(item.Hash, out _);
                return Task.CompletedTask;
            });
        }

        var hash = item.Hash;
        var kind = item.Kind;
        if (await Client.Remove(hash, kind.ToCategory(), CancellationToken.None).ConfigureAwait(false))
        {
            logger.LogInformation("Torrent [{Kind}:{Hash}] removed from TorrServer", kind, hash);
        }
        else
        {
            logger.LogWarning("Failed to remove torrent [{Kind}:{Hash}]", kind, hash);
        }
    }
}
