using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    //-------------------------------
    // This is an experimental model.
    //-------------------------------

    public class ToolRepoDistribution
    {
        public int Count { set; get; }

        public double Percentage { set; get; }

        public List<Repository> Repositories { private set; get; }

        private HashSet<int> RepositoriesID { set; get; }

        public ToolRepoDistribution()
        {
            Repositories = new List<Repository>();
            RepositoriesID = new HashSet<int>();
        }

        public void Add(Repository repository)
        {
            if (repository == null)
                return;

            Repositories.Add(repository);
            RepositoriesID.Add(repository.ID);
        }

        public void Add(IEnumerable<Repository> repositories)
        {
            if (repositories == null)
                return;

            foreach (var repository in repositories)
                Add(repository);
        }
    }
}
