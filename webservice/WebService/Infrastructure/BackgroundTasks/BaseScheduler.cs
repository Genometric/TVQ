using Genometric.TVQ.WebService.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.WebService.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.WebService.Infrastructure.BackgroundTasks
{
    public class BaseScheduler<TService, TJob> : BackgroundService
        where TService : BaseService<TJob>
        where TJob : BaseJob
    {
        protected IServiceScopeFactory ScopeFactory { get; }
        protected ILogger<BaseScheduler<TService, TJob>> Logger { get; }
        protected IBaseBackgroundTaskQueue<TJob> Queue { get; }

        public BaseScheduler(
            IServiceScopeFactory scopeFactory,
            ILogger<BaseScheduler<TService, TJob>> logger,
            IBaseBackgroundTaskQueue<TJob> queue)
        {
            ScopeFactory = scopeFactory;
            Logger = logger;
            Queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation($"{typeof(TJob)} job runner is starting.");

            /// Two separate scopes (i.e., one for reading un-finished jobs,
            /// and one for running queued jobs) are created intentionally,
            /// so that creating a separate scope per job execution could be easier.
            IServiceScope scope = ScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TVQContext>();
            foreach (var job in context.Set<TJob>()
                                       .Where(x => x.Status == State.Queued ||
                                                   x.Status == State.Running))
            {
                Queue.Enqueue(job.ID);
                Logger.LogInformation($"The unfinished job {job.ID} of type {nameof(TJob)} is re-queued.");
            }
            scope.Dispose();
            context = null;

            while (!cancellationToken.IsCancellationRequested)
            {
                var id = await Queue.DequeueAsync(cancellationToken).ConfigureAwait(false);

                try
                {
                    scope = ScopeFactory.CreateScope();
                    context = scope.ServiceProvider.GetRequiredService<TVQContext>();
                    var service = scope.ServiceProvider.GetRequiredService<TService>();
                    var job = context.Set<TJob>().Find(id);
                    await service.StartAsync(job, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    var message = $"Error occurred executing job {id} of type {nameof(id)}: {e.Message}";
                    Logger.LogError(e, message);
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
