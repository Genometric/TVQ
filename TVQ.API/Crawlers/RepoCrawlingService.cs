using Genometric.TVQ.API.Crawlers.ToolRepos;
using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.API.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Genometric.TVQ.API.Model.Repository;

namespace Genometric.TVQ.API.Crawlers
{
    public class RepoCrawlingService : BaseService<RepoCrawlingJob>
    {
        public RepoCrawlingService(
            TVQContext context,
            ILogger<RepoCrawlingService> logger) :
            base(context, logger)
        { }

        protected override async Task RunAsync(
            RepoCrawlingJob job, 
            CancellationToken cancellationToken)
        {
            Context.Entry(job).Reference(x => x.Repository).Load();

            var repo = job.Repository;
            if (repo.ToolAssociations == null)
                repo.ToolAssociations = new List<ToolRepoAssociation>();

            var tools = Context.Tools.ToList();
            var categories = Context.Categories.ToList();

            BaseToolRepoCrawler crawler;
            switch (repo.Name)
            {
                case Repo.ToolShed:
                    crawler = new ToolShed(repo, tools, categories, Logger);
                    break;

                case Repo.BioTools:
                    crawler = new BioTools(repo, tools, categories);
                    break;

                case Repo.Bioconductor:
                    crawler = new Bioconductor(repo, tools, categories);
                    break;

                default:
                    /// TODO: replace with an exception.
                    return;
            }

            await crawler.ScanAsync().ConfigureAwait(false);
            crawler.Dispose();
        }
    }
}
