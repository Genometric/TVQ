using Genometric.TVQ.API.Crawlers.ToolRepos;
using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.API.Model;
using Genometric.TVQ.API.Model.Associations;
using Microsoft.EntityFrameworkCore;
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

        protected override async Task ExecuteAsync(
            RepoCrawlingJob job,
            CancellationToken cancellationToken)
        {
            // TODO: each of the following loads has performance penalty;
            // hence they should be limited to only the needed ones. Carefully 
            // check which ones are required, and remove the unessential ones.
            Context.Entry(job).Reference(x => x.Repository).Load();

            var repo = job.Repository;
            Context.Entry(repo).Collection(x => x.ToolAssociations).Load();
            if (repo.ToolAssociations == null)
                repo.ToolAssociations = new List<ToolRepoAssociation>();

            var tools = Context.Tools.Include(x => x.RepoAssociations).ToList();
            var categories = Context.Categories.Include(x => x.ToolAssociations).ToList();
            var publications = Context.Publications.Include(x => x.ToolAssociations).ToList();

            BaseToolRepoCrawler crawler;
            switch (repo.Name)
            {
                case Repo.ToolShed:
                    crawler = new ToolShed(repo, tools, publications, categories, Logger);
                    break;

                case Repo.BioTools:
                    crawler = new BioTools(repo, tools, publications, categories);
                    break;

                case Repo.Bioconductor:
                    crawler = new Bioconductor(repo, tools, publications, categories);
                    break;

                case Repo.Bioconda:
                    crawler = new Bioconda(repo, tools, publications, categories);
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
