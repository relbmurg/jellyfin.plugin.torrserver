using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.TorrServer.Core
{
    internal sealed class PendingItem(string hash, MediaKind kind, CancellationToken cancellation) : IDisposable
    {
        private readonly CancellationTokenSource _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        private Task? _job;

        public string Hash { get; } = hash;

        public MediaKind Kind { get; } = kind;

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            _job?.Dispose();
        }

        public void Start(TimeSpan grace, Func<PendingItem, CancellationToken, Task> callback)
        {
            if (!_cts.IsCancellationRequested)
            {
                _job = Do(grace, callback);
            }
        }

        private async Task Do(TimeSpan grace, Func<PendingItem, CancellationToken, Task> callback)
        {
            try
            {
                await Task.Delay(grace, _cts.Token).ConfigureAwait(false);
                await callback(this, _cts.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                _cts.Dispose();
            }
        }
    }
}
