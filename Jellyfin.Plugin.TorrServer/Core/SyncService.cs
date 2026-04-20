using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TorrServer.Client;
using Jellyfin.Plugin.TorrServer.Core.Parser;
using Jellyfin.Plugin.TorrServer.Extensions;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TorrServer.Core;

/// <inheritdoc />
internal class SyncService(IPluginConfigurationProvider configProvider, ILibraryManager libraryManager, IApiClient client, ILogger<SyncService> logger) : ISyncService
{
    private readonly FolderNameParser _folderNameParser = new();

    private readonly FileNameParser _fileNameParser = new();

    /// <inheritdoc />
    public async Task ProcessTorrents(CancellationToken cancellation)
    {
        logger.LogInformation("Start");

        var config = configProvider.Get();

        var moviesRoots = libraryManager.GetLibraryPaths(config.MoviesLibraryId);
        var showsRoots = libraryManager.GetLibraryPaths(config.TvShowsLibraryId);

        if (moviesRoots.Length == 0 && showsRoots.Length == 0)
        {
            logger.LogWarning("Library paths are missing. Skipping");
            return;
        }

        var existed = GetExistedHashes(moviesRoots.Concat(showsRoots));

        var torrents = await client.List(cancellation).ConfigureAwait(false);

        foreach (var item in torrents.Where(x => x.Category is Category.Movie or Category.Tv && !existed.Contains(x.Hash)).OrderBy(x => x.Timestamp))
        {
            var root = item.Category switch
            {
                Category.Movie => moviesRoots.FirstOrDefault(x => x.Equals(config.MoviesLocation, StringComparison.OrdinalIgnoreCase)),
                Category.Tv => showsRoots.FirstOrDefault(x => x.Equals(config.TvShowsLocation, StringComparison.OrdinalIgnoreCase)),
                _ => null
            };

            if (string.IsNullOrWhiteSpace(root))
            {
                continue;
            }

            using var scope = logger.BeginScope(new[] { ("Title", item.Title), ("Hash", item.Hash) });
            logger.LogInformation("Start processing");

            var folder = _folderNameParser.Parse(item.Title).FolderName().ToSafeFileName('.');
            logger.LogInformation("Folder name -> {Folder}", folder);

            var playlist = await client.GetPlaylist(item.Hash, cancellation).ConfigureAwait(false);
            logger.LogInformation("Playlist received. Items: {ItemsCount}", playlist.Entries.Length);

            if (playlist.Entries.Length == 0)
            {
                logger.LogWarning("Playlist items are missing, skipping.");
                continue;
            }

            await CreateMediaEntry(Path.Combine(root, folder), playlist, item.Category, item.Hash, cancellation: cancellation).ConfigureAwait(false);
        }

        logger.LogInformation("Stop");
        return;

        static HashSet<string> GetExistedHashes(IEnumerable<string> roots)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var options = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,
                ReturnSpecialDirectories = false
            };

            foreach (var root in roots)
            {
                if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                {
                    continue;
                }

                foreach (var file in Directory.EnumerateFiles(root, "*", options))
                {
                    if (!Path.HasExtension(file))
                    {
                        var info = new FileInfo(file);
                        if (info.Length == 0)
                        {
                            result.Add(info.Name);
                        }
                    }
                }
            }

            return result;
        }
    }

    private async Task CreateMediaEntry(string directory, Playlist playlist, Category category, string hash, string[]? parameters = null, CancellationToken cancellation = default)
    {
        var additionalParameters = string.Join("&", parameters ?? []);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(Path.Combine(directory, hash)))
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

        File.Create(Path.Combine(directory, hash)).Close();
        logger.LogInformation("Hash file saved");
    }
}
