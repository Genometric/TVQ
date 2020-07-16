using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.WebService.Infrastructure.BackgroundTasks
{
    public class BaseBackgroundTaskQueue<T> : IBaseBackgroundTaskQueue<T>, IDisposable
    {
        private readonly SemaphoreSlim _signal;
        private readonly ConcurrentQueue<int> _jobs;

        public BaseBackgroundTaskQueue()
        {
            _signal = new SemaphoreSlim(0);
            _jobs = new ConcurrentQueue<int>();
        }

        public void Enqueue(int id)
        {
            _jobs.Enqueue(id);
            _signal.Release();
        }

        public async Task<int> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);
            _jobs.TryDequeue(out int id);
            return id;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Managed resources to be disposed.
                if (_signal != null)
                    _signal.Dispose();
            }

            // Native resources to be disposed.
        }
    }
}
