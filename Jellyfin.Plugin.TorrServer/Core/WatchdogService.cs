using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TorrServer.Abstractions;
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
    private readonly ConcurrentDictionary<string, PendingItem> _pending = new(StringComparer.OrdinalIgnoreCase);
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
                if (_pending.TryRemove(hash, out var item))
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
                if (_pending.TryRemove(hash, out var existing))
                {
                    existing.Dispose();
                }

                var item = new PendingItem(hash, kind, _cts.Token);
                if (_pending.TryAdd(hash, item))
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

    public async Task ProcessTorrents(CancellationToken cancellation)
    {
        logger.LogInformation("Start");

        var config = Config;

        var moviesRoots = libraryManager.GetLibraryPaths(config.MoviesLibraryId);
        var showsRoots = libraryManager.GetLibraryPaths(config.TvShowsLibraryId);

        if (moviesRoots.Length == 0 && showsRoots.Length == 0)
        {
            logger.LogWarning("Library paths are missing. Skipping");
            return;
        }

        var roots = new Dictionary<Category, string?>()
        {
            { Category.Movie, moviesRoots.FirstOrDefault(x => x.Equals(config.MoviesLocation, StringComparison.OrdinalIgnoreCase)) },
            { Category.Tv,  showsRoots.FirstOrDefault(x => x.Equals(config.TvShowsLocation, StringComparison.OrdinalIgnoreCase)) },
        };

        var existed = TorrentHashHelper.GetExistedHashes(moviesRoots.Concat(showsRoots));

        var pending = _pending.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var torrents = await Client.List(cancellation).ConfigureAwait(false);

        var filtered = torrents
            .Where(x =>
                x.Category is Category.Movie or Category.Tv
                && !existed.Contains(x.Hash)
                && !pending.Contains(x.Hash));

        foreach (var item in filtered)
        {
            if (!roots.TryGetValue(item.Category, out var root) || string.IsNullOrWhiteSpace(root))
            {
                continue;
            }

            using var scope = logger.BeginScope(new[] { ("Title", item.Title), ("Hash", item.Hash) });
            logger.LogInformation("Start processing");

            var folder = _folderNameParser.Parse(item.Title).FolderName().ToSafeFileName('.');
            logger.LogInformation("Folder name -> {Folder}", folder);

            var playlist = await Client.GetPlaylist(item.Hash, cancellation).ConfigureAwait(false);
            logger.LogInformation("Playlist received. Items: {ItemsCount}", playlist.Entries.Length);

            if (playlist.Entries.Length == 0)
            {
                logger.LogWarning("Playlist items are missing, skipping.");
                continue;
            }

            await CreateMediaEntry(Path.Combine(root, folder), playlist, item.Category, item.Hash, cancellation: cancellation).ConfigureAwait(false);
        }

        logger.LogInformation("Stop");
    }

    private async Task CreateMediaEntry(string directory, Playlist playlist, Category category, string hash, string[]? parameters = null, CancellationToken cancellation = default)
    {
        var additionalParameters = string.Join("&", parameters ?? []);

        Directory.CreateDirectory(directory);

        var hashFilename = hash.GetHashFileName();
        if (File.Exists(Path.Combine(directory, hashFilename)))
        {
            logger.LogInformation("Already saved. Skipping");
            return;
        }

        foreach (var entry in playlist.Entries)
        {
            var title = Path.GetFileName(entry.Title);
            var titlePath = Path.GetDirectoryName(entry.Title) ?? string.Empty;
            var file = _fileNameParser.Parse(title).StrmFileName().ToSafeFileName(' ');
            var path = Path.Combine(directory, titlePath, file);
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
                path = Path.Combine(directory, titlePath, $"{name}.{i}{ext}");
                i++;
            }

            Directory.CreateDirectory(Path.Combine(directory, titlePath));
            await File.WriteAllTextAsync(path, url, cancellation).ConfigureAwait(false);
            logger.LogInformation("Playlist entry saved to {Location}", path);
        }

        File.Create(Path.Combine(directory, hashFilename)).Close();
        logger.LogInformation("Hash file saved");
    }

    private async Task DeleteTorrent(PendingItem item, CancellationToken cancellation)
    {
        if (!_pending.TryRemove(item.Hash, out _))
        {
            return;
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
