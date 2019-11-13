using Genometric.TVQ.API.Crawlers.Literature;
using Genometric.TVQ.API.Crawlers.ToolRepos;
using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Genometric.TVQ.API.Model.Repository;

namespace Genometric.TVQ.API.Crawlers
{
    public class CrawlerService
    {
        private readonly TVQContext _dbContext;
        private readonly ILogger<CrawlerService> _logger;

        public CrawlerService(
            TVQContext dbContext,
            ILogger<CrawlerService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task CrawlAsync(Repository repo, CancellationToken cancellationToken)
        {
            // TODO: check if another async operation is ongoing, if so, wait for that to finish before running this. 
            try
            {
                if (!_dbContext.Repositories.Local.Any(e => e.ID == repo.ID))
                    _dbContext.Attach(repo);

                if (repo.Tools == null)
                    repo.Tools = new List<Tool>();

                var tools = repo.Tools.ToList();

                BaseToolRepoCrawler crawler;
                switch (repo.Name)
                {
                    case Repo.ToolShed:
                        crawler = new ToolShed(repo, _logger);
                        break;

                    case Repo.BioTools:
                        crawler = new BioTools(repo);
                        break;

                    case Repo.Bioconductor:
                        crawler = new Bioconductor(repo);
                        break;

                    default:
                        /// TODO: replace with an exception.
                        return;
                }

                await crawler.ScanAsync().ConfigureAwait(false);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);

                crawler.Dispose();
            }
            catch (DbUpdateConcurrencyException e)
            {
                // TODO log this.
                throw;
            }
            catch(DbUpdateException e)
            {
                // TODO log this. 
                throw;
            }
            catch(Exception e)
            {
                // TODO log this.
                throw;
            }
        }

        public async Task CrawlAsync(List<Publication> publications, CancellationToken cancellationToken)
        {
            try
            {
                _dbContext.AttachRange(publications);

                using var scopusCrawler = new Scopus(publications, _logger);
                await scopusCrawler.CrawlAsync().ConfigureAwait(false);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch(Exception e)
            {

            }
        }
    }
}
