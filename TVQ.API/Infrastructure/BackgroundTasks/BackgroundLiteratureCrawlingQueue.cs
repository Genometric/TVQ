using Genometric.TVQ.API.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public class BackgroundLiteratureCrawlingQueue : IBackgroundLiteratureCrawlingQueue
    {
        private readonly ConcurrentQueue<List<Publication>> _publicationsToScan =
            new ConcurrentQueue<List<Publication>>();
        private readonly SemaphoreSlim _signal =
            new SemaphoreSlim(0);

        public void QueueBackgroundWorkItem(List<Publication> publications)
        {
            if (publications == null)
                throw new ArgumentNullException(nameof(publications));

            _publicationsToScan.Enqueue(publications);
            _signal.Release();
        }

        public async Task<List<Publication>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);
            _publicationsToScan.TryDequeue(out var repository);

            return repository;
        }
    }
}
