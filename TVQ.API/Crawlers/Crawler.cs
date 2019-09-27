using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using static Genometric.TVQ.API.Model.Repository;

namespace Genometric.TVQ.API.Crawlers
{
    public class Crawler
    {
        private readonly TVQContext _dbContext;

        public Crawler(TVQContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CrawlAsync(Repository repo)
        {
            try
            {
                ToolRepoCrawler crawler;
                switch (repo.Name)
                {
                    case Repo.ToolShed:
                        crawler = new ToolShed(_dbContext, repo);
                        break;

                    case Repo.BioTools:
                        crawler = new BioTools(_dbContext, repo);
                        break;

                    default:
                        /// TODO: replace with an exception.
                        return;
                }

                await crawler.ScanAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }
    }
}
