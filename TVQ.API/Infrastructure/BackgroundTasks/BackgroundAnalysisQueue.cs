using Genometric.TVQ.API.Model;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public class BackgroundAnalysisQueue : IBackgroundAnalysisTaskQueue
    {
        private readonly ConcurrentQueue<AnalysisJob> _jobs = 
            new ConcurrentQueue<AnalysisJob>();
        private readonly SemaphoreSlim _signal =
            new SemaphoreSlim(0);

        public void QueueBackgroundWorkItem(AnalysisJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            _jobs.Enqueue(job);
            _signal.Release();
        }

        public async Task<AnalysisJob> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);
            _jobs.TryDequeue(out var job);

            return job;
        }
    }
}
