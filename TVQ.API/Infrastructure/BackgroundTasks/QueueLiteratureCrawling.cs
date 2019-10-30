using Genometric.TVQ.API.Crawlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public class QueueLiteratureCrawling : BackgroundService
    {
        private ILogger<QueueLiteratureCrawling> _logger;
        private IServiceProvider Services { get; }
        public IBackgroundLiteratureCrawlingQueue CrawlingQueue { get; }

        public QueueLiteratureCrawling(
            IBackgroundLiteratureCrawlingQueue crawlingQueue,
            IServiceProvider services,
            ILogger<QueueLiteratureCrawling> logger)
        {
            Services = services;
            _logger = logger;
            CrawlingQueue = crawlingQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued Hosted Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var publications = await CrawlingQueue.DequeueAsync(stoppingToken).ConfigureAwait(false);

                try
                {
                    using (var scope = Services.CreateScope())
                    {
                        var scopedProcessingService = scope
                            .ServiceProvider
                            .GetRequiredService<CrawlerService>();

                        await scopedProcessingService
                            .CrawlAsync(publications, stoppingToken)
                            .ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                       $"Error occurred executing {nameof(publications)}.");
                }
            }

            _logger.LogInformation("Queued Hosted Service is stopping.");
        }
    }
}
