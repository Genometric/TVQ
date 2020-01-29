using Genometric.TVQ.API.Crawlers.Literature;
using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.API.Model;
using Microsoft.Extensions.Logging;
using System.Linq;
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

        protected override async Task ExecuteAsync(
            LiteratureCrawlingJob job,
            CancellationToken cancellationToken)
        {
            if(job.ScanAllPublications)
                job.Publications = Context.Publications.ToList();
            
            Context.Entry(job).Collection(x => x.Publications).Load();
            using var scopusCrawler = new Scopus(job.Publications, Logger);
            await scopusCrawler.CrawlAsync().ConfigureAwait(false);
        }
    }
}
