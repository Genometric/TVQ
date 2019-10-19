using Genometric.TVQ.API.Model;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public class BackgroundCrawlingQueue : IBackgroundCrawlingQueue
    {
        private readonly ConcurrentQueue<Repository> _repositoriesToScan =
            new ConcurrentQueue<Repository>();
        private readonly SemaphoreSlim _signal =
            new SemaphoreSlim(0);

        public void QueueBackgroundWorkItem(Repository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            _repositoriesToScan.Enqueue(repository);
            _signal.Release();
        }

        public async Task<Repository> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);
            _repositoriesToScan.TryDequeue(out var repository);

            return repository;
        }
    }
}
