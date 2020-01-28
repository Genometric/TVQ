using Genometric.TVQ.API.Crawlers.Literature;
using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.API.Model;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Crawlers
{
    public class LiteratureCrawlingService : BaseService<LiteratureCrawlingJob>
    {
        public LiteratureCrawlingService(
            TVQContext context,
            ILogger<LiteratureCrawlingService> logger) :
            base(context, logger)
        { }

        protected override async Task RunAsync(
            LiteratureCrawlingJob job,
            CancellationToken cancellationToken)
        {
            using var scopusCrawler = new Scopus(job.Publications, Logger);
            await scopusCrawler.CrawlAsync().ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
