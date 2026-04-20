using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TorrServer.Extensions;

namespace Jellyfin.Plugin.TorrServer.Client;

internal class Playlist
{
    private const string Header = "#EXTM3U";

    private const string EntryHeader = "#EXTINF";

    private readonly List<PlaylistEntry> _entries = [];

    public PlaylistEntry[] Entries => _entries.ToArray();

    public static Playlist Empty { get; } = new();

    public static async Task<Playlist> Parse(string source, CancellationToken cancellation)
    {
        var result = new Playlist();
        if (string.IsNullOrWhiteSpace(source))
        {
            return result;
        }

        using var reader = new StringReader(source);

        // #EXTM3U
        var line = await reader.ReadLineAsync(cancellation).ConfigureAwait(false);
        if (line == null || !line.StartsWith(Header, StringComparison.OrdinalIgnoreCase))
        {
            return result;
        }

        while (true)
        {
            line = await reader.ReadLineAsync(cancellation).ConfigureAwait(false);
            if (line == null)
            {
                break;
            }

            // #EXTINF
            if (!line.StartsWith(EntryHeader, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var title = line[(line.IndexOf(',', StringComparison.InvariantCulture) + 1)..];
            while (true)
            {
                line = await reader.ReadLineAsync(cancellation).ConfigureAwait(false);
                if (line == null)
                {
                    break;
                }

                // Additional tags
                if (line.StartsWith('#'))
                {
                    continue;
                }

                break;
            }

            if (Uri.TryCreate(line, UriKind.Absolute, out var uri))
            {
                result._entries.Add(new PlaylistEntry(title, uri));
            }
        }

        return result;
    }

    public static Playlist Create(Uri baseUri, TorrentItem item)
    {
        var result = new Playlist();
        result._entries.AddRange(
            item.Files
                .Select(x =>
                    new PlaylistEntry(
                        x.Path.RemoveFirstSegment(),
                        new Uri(baseUri, $"stream/{Path.GetFileName(x.Path)}?link={item.Hash}&index={x.Id}&play"))));

        return result;
    }

    internal record PlaylistEntry(string Title, Uri Uri);
}
