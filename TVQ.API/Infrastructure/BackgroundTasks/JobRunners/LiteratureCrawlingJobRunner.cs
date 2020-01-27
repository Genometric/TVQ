using Genometric.TVQ.API.Crawlers;
using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public class LiteratureCrawlingJobRunner : BaseJobRunner<LiteratureCrawlingJob>
    {
        public LiteratureCrawlingJobRunner(
            TVQContext context,
            IServiceProvider services,
            ILogger<LiteratureCrawlingJobRunner> logger,
            IBaseBackgroundTaskQueue<LiteratureCrawlingJob> queue) :
            base(context,
                 services,
                 logger,
                 queue)
        { }

        protected override IQueryable<LiteratureCrawlingJob> GetPendingJobs()
        {
            return Context.LiteratureCrawlingJobs.Include(x => x.Publications)
                                                 .Where(x => x.Status == State.Queued ||
                                                             x.Status == State.Running);
        }

        protected override LiteratureCrawlingJob AugmentJob(LiteratureCrawlingJob job)
        {
            return job;
        }

        protected override Task RunJobAsync(
            IServiceScope scope,
            LiteratureCrawlingJob job,
            CancellationToken cancellationToken)
        {
            var service = scope.ServiceProvider.GetRequiredService<CrawlerService>();
            return service.CrawlAsync(job, cancellationToken);
        }
    }
}
