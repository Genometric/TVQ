using Genometric.TVQ.API.Crawlers;
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
    public class QueueToolRepoCrawling : BackgroundService
    {
        private ILogger<QueueToolRepoCrawling> _logger;
        private TVQContext Context { get; }
        private IServiceProvider Services { get; }
        public IBaseBackgroundTaskQueue<RepoCrawlingJob> CrawlingQueue { get; }

        public QueueToolRepoCrawling(
            TVQContext context,
            IBaseBackgroundTaskQueue<RepoCrawlingJob> crawlingQueue,
            IServiceProvider services,
            ILogger<QueueToolRepoCrawling> logger)
        {
            Services = services;
            _logger = logger;
            CrawlingQueue = crawlingQueue;
            Context = context;
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ToolRepoCrawling hosted service is starting.");

            var unfinishedJobs = Context.RepoCrawlingJobs
                .Where(x => x.Status == Model.State.Queued ||
                            x.Status == Model.State.Running);

            foreach (var job in unfinishedJobs)
            {
                CrawlingQueue.Enqueue(job);
                _logger.LogInformation($"The unfinished tool repository crawling job {job.ID} is re-queued.");
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                var job = await CrawlingQueue.DequeueAsync(cancellationToken).ConfigureAwait(false);

                IServiceScope scope = null;
                try
                {
                    scope = Services.CreateScope();
                    var scopedProcessingService = scope
                        .ServiceProvider
                        .GetRequiredService<CrawlerService>();

                    await scopedProcessingService
                        .CrawlAsync(job, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                       $"Error occurred executing {nameof(job)}.");
                }
                finally
                {
                    if (scope != null)
                        scope.Dispose();
                }
            }

            _logger.LogInformation("Queued Hosted Service is stopping.");
        }
    }
}
