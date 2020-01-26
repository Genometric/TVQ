using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public class BaseBackgroundTaskQueue<T> : IBaseBackgroundTaskQueue<T>, IDisposable
    {
        private readonly SemaphoreSlim _signal;
        private readonly ConcurrentQueue<T> _jobs;

        public BaseBackgroundTaskQueue()
        {
            _signal = new SemaphoreSlim(0);
            _jobs = new ConcurrentQueue<T>();
        }

        public void Enqueue(T job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            _jobs.Enqueue(job);
            _signal.Release();
        }

        public async Task<T> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);
            _jobs.TryDequeue(out var job);
            return job;
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
