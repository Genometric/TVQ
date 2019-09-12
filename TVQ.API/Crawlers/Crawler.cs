using Genometric.TVQ.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Genometric.TVQ.API.Model.Repository;

namespace Genometric.TVQ.API.Crawlers
{
    public class Crawler
    {
        public Crawler() { }

        public async Task<List<Tool>> GetToolsAsync(Repository repo)
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

        public async Task<List<Publication>> GetPublicationsAsync(Repository repo, List<Tool> tools)
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
