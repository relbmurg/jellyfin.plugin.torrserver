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

    /// <summary>
    /// Remove torrent by hash.
    /// </summary>
    /// <param name="hash">Torrent hash.</param>
    /// <param name="category">Torrent category.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    Task<bool> Remove(string hash, Category category, CancellationToken cancellation);

    /// <summary>
    /// Gets current torrserver settings.
    /// </summary>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>TorrServer configuration.</returns>
    Task<ServerConfig> GetConfiguration(CancellationToken cancellation);
}
