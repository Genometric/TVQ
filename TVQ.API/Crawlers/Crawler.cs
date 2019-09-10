using Genometric.TVQ.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Genometric.TVQ.API.Model.Repository;

namespace Genometric.TVQ.API.Crawlers
{
    public class Crawler
    {
        public Crawler() { }

        public async Task<List<Tool>> CrawlAsync(Repository repo)
        {
            switch (repo.Name)
            {
                case Repo.ToolShed:
                    return await new ToolShed().Crawl(repo);

                default:
                    /// TODO: replace with an exception.
                    return new List<Tool>();
            }
        }
    }
}
