using Genometric.TVQ.API.Crawlers;
using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public class QueueLiteratureCrawling : BackgroundService
    {
        private ILogger<QueueLiteratureCrawling> _logger;
        private IServiceProvider Services { get; }
        public IBaseBackgroundTaskQueue<LiteratureCrawlingJob> CrawlingQueue { get; }
        private TVQContext Context { get; }

        public QueueLiteratureCrawling(
            TVQContext context,
            IBaseBackgroundTaskQueue<LiteratureCrawlingJob> crawlingQueue,
            IServiceProvider services,
            ILogger<QueueLiteratureCrawling> logger)
        {
            Context = context;
            Services = services;
            _logger = logger;
            CrawlingQueue = crawlingQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LiteratureCrawling hosted service is starting.");

            var unfinishedJobs = Context.LiteratureCrawlingJobs.Include(x => x.Publications)
                .Where(x => x.Status == Model.State.Queued ||
                            x.Status == Model.State.Running);

            foreach (var job in unfinishedJobs)
            {
                CrawlingQueue.Enqueue(job);
                _logger.LogInformation($"The unfinished literature crawling job {job.ID} is re-queued.");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var job = await CrawlingQueue.DequeueAsync(stoppingToken).ConfigureAwait(false);

                IServiceScope scope = null;
                try
                {
                    scope = Services.CreateScope();
                    var scopedProcessingService = scope
                        .ServiceProvider
                        .GetRequiredService<CrawlerService>();

                    await scopedProcessingService
                        .CrawlAsync(job, stoppingToken)
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
