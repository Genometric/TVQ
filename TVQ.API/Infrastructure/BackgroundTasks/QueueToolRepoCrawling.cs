using Genometric.TVQ.API.Crawlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public class QueueToolRepoCrawling : BackgroundService
    {
        private ILogger<QueueToolRepoCrawling> _logger;
        private IServiceProvider Services { get; }
        public IBackgroundToolRepoCrawlingQueue CrawlingQueue { get; }

        public QueueToolRepoCrawling(
            IBackgroundToolRepoCrawlingQueue crawlingQueue,
            IServiceProvider services,
            ILogger<QueueToolRepoCrawling> logger)
        {
            Services = services;
            _logger = logger;
            CrawlingQueue = crawlingQueue;
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Queued Hosted Service is starting.");

            while (!cancellationToken.IsCancellationRequested)
            {
                var repository = await CrawlingQueue.DequeueAsync(cancellationToken).ConfigureAwait(false);

                try
                {
                    using (var scope = Services.CreateScope())
                    {
                        var scopedProcessingService = scope
                            .ServiceProvider
                            .GetRequiredService<CrawlerService>();

                        await scopedProcessingService
                            .CrawlAsync(repository, cancellationToken)
                            .ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                       $"Error occurred executing {nameof(repository)}.");
                }
            }

            _logger.LogInformation("Queued Hosted Service is stopping.");
        }
    }
}
