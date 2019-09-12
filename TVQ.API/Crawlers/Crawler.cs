using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
            var tools = await GetToolsAsync(repo);
            var publs = await GetPublicationsAsync(repo, tools);

            try
            {
                await _dbContext.Tools.AddRangeAsync(tools);
                await _dbContext.Publications.AddRangeAsync(publs);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        private async Task<List<Tool>> GetToolsAsync(Repository repo)
        {
            switch (repo.Name)
            {
                case Repo.ToolShed:
                    return await new ToolShed().GetTools(repo);

                default:
                    /// TODO: replace with an exception.
                    return new List<Tool>();
            }
        }

        private async Task<List<Publication>> GetPublicationsAsync(Repository repo, List<Tool> tools)
        {
            switch (repo.Name)
            {
                case Repo.ToolShed:
                    return await new ToolShed().GetPublications(repo, tools);

                default:
                    /// TODO: replace with an exception.
                    return new List<Publication>();
            }
        }
    }
}
