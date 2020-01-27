using Genometric.TVQ.API.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public abstract class BaseJobRunner<T> : BackgroundService
        where T : BaseJob
    {
        protected TVQContext Context { get; }
        protected IServiceProvider Services { get; }
        protected ILogger<BaseJobRunner<T>> Logger { get; }
        protected IBaseBackgroundTaskQueue<T> Queue { get; }

        protected BaseJobRunner(
            TVQContext context,
            IServiceProvider services,
            ILogger<BaseJobRunner<T>> logger,
            IBaseBackgroundTaskQueue<T> queue
            )
        {
            Context = context;
            Services = services;
            Logger = logger;
            Queue = queue;
        }

        protected abstract IQueryable<T> GetPendingJobs();

        protected abstract T AugmentJob(T job);

        protected abstract Task RunJobAsync(IServiceScope scope, T job, CancellationToken cancellationToken);

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation($"{nameof(T)} job runner is starting.");

            foreach (var job in GetPendingJobs())
            {
                Queue.Enqueue(job);
                Logger.LogInformation($"The unfinished job {job.ID} of type {nameof(T)} is re-queued.");
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                IServiceScope scope = null;
                var dequeuedJob = await Queue.DequeueAsync(cancellationToken).ConfigureAwait(false);
                var job = AugmentJob(dequeuedJob);

                try
                {
                    scope = Services.CreateScope();
                    await RunJobAsync(scope, job, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Logger.LogError(e,
                       $"Error occurred executing job {job.ID} of type {nameof(job)}.");
                }
                finally
                {
                    if (scope != null)
                        scope.Dispose();
                }
            }

            Logger.LogInformation($"{nameof(T)} job runner is stopping.");
        }
    }
}
