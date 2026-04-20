using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.TorrServer.Client;

internal interface IApiClient
{
    /// <summary>
    /// Gets list of torrents.
    /// </summary>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>Array of torrent items.</returns>
    Task<TorrentItem[]> List(CancellationToken cancellation);

    /// <summary>
    /// Gets playlist for specified hash.
    /// </summary>
    /// <param name="hash">Torrent hash.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>Playlist for torrent.</returns>
    Task<Playlist> GetPlaylist(string hash, CancellationToken cancellation);
}
