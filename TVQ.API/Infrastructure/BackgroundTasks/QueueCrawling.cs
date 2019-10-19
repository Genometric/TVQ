using Genometric.TVQ.API.Crawlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public class QueueCrawling : BackgroundService
    {
        private ILogger<QueueCrawling> _logger;
        private IServiceProvider Services { get; }
        public IBackgroundCrawlingQueue CrawlingQueue { get; }

        public QueueCrawling(
            IBackgroundCrawlingQueue crawlingQueue,
            IServiceProvider services,
            ILogger<QueueCrawling> logger)
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
                            .GetRequiredService<Crawler>();

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
