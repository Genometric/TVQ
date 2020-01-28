using Genometric.TVQ.API.Infrastructure.BackgroundTasks.JobRunners;
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
    public class BaseScheduler<TService, TJob> : BackgroundService
        where TService : BaseService<TJob>
        where TJob : BaseJob
    {
        protected IServiceProvider Services { get; }
        protected IServiceScopeFactory ScopeFactory { get; }
        protected ILogger<BaseScheduler<TService, TJob>> Logger { get; }
        protected IBaseBackgroundTaskQueue<TJob> Queue { get; }

        public BaseScheduler(
            IServiceProvider services,
            IServiceScopeFactory scopeFactory,
            ILogger<BaseScheduler<TService, TJob>> logger,
            IBaseBackgroundTaskQueue<TJob> queue)
        {
            Services = services;
            ScopeFactory = scopeFactory;
            Logger = logger;
            Queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation($"{typeof(TJob)} job runner is starting.");

            using (var scope = ScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TVQContext>();
                foreach (var job in context.Set<TJob>()
                                           .Where(x => x.Status == State.Queued ||
                                                       x.Status == State.Running))
                {
                    Queue.Enqueue(job);
                    Logger.LogInformation($"The unfinished job {job.ID} of type {nameof(TJob)} is re-queued.");
                }
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                IServiceScope scope = null;
                var job = await Queue.DequeueAsync(cancellationToken).ConfigureAwait(false);

                try
                {
                    scope = Services.CreateScope();
                    var servcie = scope.ServiceProvider.GetRequiredService<TService>();
                    await servcie.ExecuteAsync(job, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    var msg = $"Error occurred executing job {job.ID} of type {nameof(job)}: {e.Message}";
                    Logger.LogError(e, msg);
                }
                finally
                {
                    if (scope != null)
                        scope.Dispose();
                }
            }

            Logger.LogInformation($"{typeof(TJob)} job runner is stopping.");
        }
    }
}
