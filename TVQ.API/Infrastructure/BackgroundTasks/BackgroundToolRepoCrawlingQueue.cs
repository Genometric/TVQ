using Genometric.TVQ.API.Model;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public class BackgroundToolRepoCrawlingQueue : IBackgroundToolRepoCrawlingQueue
    {
        private readonly ConcurrentQueue<RepoCrawlingJob> _repositoriesToScan =
            new ConcurrentQueue<RepoCrawlingJob>();
        private readonly SemaphoreSlim _signal =
            new SemaphoreSlim(0);

        public void QueueBackgroundWorkItem(RepoCrawlingJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            _repositoriesToScan.Enqueue(job);
            _signal.Release();
        }

        public async Task<RepoCrawlingJob> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);
            _repositoriesToScan.TryDequeue(out var job);

            return job;
        }
    }
}
