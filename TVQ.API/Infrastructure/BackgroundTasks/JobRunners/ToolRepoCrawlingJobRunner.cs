using Genometric.TVQ.API.Crawlers;
using Genometric.TVQ.API.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public class ToolRepoCrawlingJobRunner : BaseJobRunner<RepoCrawlingJob>
    {
        public ToolRepoCrawlingJobRunner(
            TVQContext context,
            IServiceProvider services,
            ILogger<ToolRepoCrawlingJobRunner> logger,
            IBaseBackgroundTaskQueue<RepoCrawlingJob> queue) : 
            base(context,
                 services,
                 logger,
                 queue)
        { }

        protected override RepoCrawlingJob AugmentJob(RepoCrawlingJob job)
        {
            return job;
        }

        protected override Task RunJobAsync(
            IServiceScope scope,
            RepoCrawlingJob job,
            CancellationToken cancellationToken)
        {
            var servcie = scope.ServiceProvider.GetRequiredService<CrawlerService>();
            return servcie.CrawlAsync(job, cancellationToken);
        }
    }
}
