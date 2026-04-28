using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.TorrServer.Core;

/// <summary>
/// Sync torrents task.
/// </summary>
public class SyncTask(ISyncService service) : IScheduledTask
{
    /// <inheritdoc />
    public string Name => "Sync TorrServer Library";

    /// <inheritdoc />
    public string Key => "TorrServerSync";

    /// <inheritdoc />
    public string Description => "Saves torrents from TorrServer to STRM files";

    /// <inheritdoc />
    public string Category => "TorrServer";

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        await service.ProcessTorrents(progress, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return [new TaskTriggerInfo { Type = TaskTriggerInfoType.IntervalTrigger, IntervalTicks = TimeSpan.FromMinutes(2).Ticks }];
    }
}
