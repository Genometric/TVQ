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

        public async Task CrawlAsync(RepoCrawlingJob job, CancellationToken cancellationToken)
        {
            // TODO: check if another async operation is ongoing, if so, wait for that to finish before running this. 
            try
            {
                job.Status = State.Running;
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);

                var repo = job.Repository;
                if (!_dbContext.RepoCrawlingJobs.Local.Any(e => e.ID == job.ID))
                    _dbContext.Attach(job);
                if (!_dbContext.Repositories.Local.Any(e => e.ID == repo.ID))
                    _dbContext.Attach(repo);

                if (repo.ToolAssociations == null)
                    repo.ToolAssociations = new List<ToolRepoAssociation>();

                var tools = _dbContext.Tools.ToList();
                var categories = _dbContext.Categories.ToList();

                BaseToolRepoCrawler crawler;
                switch (repo.Name)
                {
                    case Repo.ToolShed:
                        crawler = new ToolShed(repo, tools, categories, _logger);
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
                job.Status = State.Completed;
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
                crawler.Dispose();
            }
            catch (DbUpdateConcurrencyException e)
            {
                // TODO log this.
                throw;
            }
            catch (DbUpdateException e)
            {
                // TODO log this. 
                throw;
            }
            catch (Exception e)
            {
                // TODO log this.
                throw;
            }
        }

        public async Task CrawlAsync(LiteratureCrawlingJob job, CancellationToken cancellationToken)
        {
            try
            {
                job.Status = State.Running;
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);

                using var scopusCrawler = new Scopus(job.Publications, _logger);
                await scopusCrawler.CrawlAsync().ConfigureAwait(false);
                job.Status = State.Completed;
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {

            }
        }
    }
}
