using Genometric.TVQ.API.Model;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public class BackgroundLiteratureCrawlingQueue : IBackgroundLiteratureCrawlingQueue
    {
        private readonly ConcurrentQueue<LiteratureCrawlingJob> _jobs =
            new ConcurrentQueue<LiteratureCrawlingJob>();
        private readonly SemaphoreSlim _signal =
            new SemaphoreSlim(0);

        public void QueueBackgroundWorkItem(LiteratureCrawlingJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            _jobs.Enqueue(job);
            _signal.Release();
        }

        public async Task<LiteratureCrawlingJob> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);
            _jobs.TryDequeue(out var repository);

            return repository;
        }
    }
}
