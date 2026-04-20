using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.TorrServer.Core;

/// <summary>
/// Synchronization service.
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// Check new torrents.
    /// </summary>
    /// <param name="cancellation">Instance of the <see cref="CancellationToken"/>.</param>
    /// <returns>Instance of the <see cref="Task"/>.</returns>
    Task ProcessTorrents(CancellationToken cancellation);
}
