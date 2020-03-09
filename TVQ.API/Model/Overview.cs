namespace Genometric.TVQ.API.Model
{
    public class Overview : BaseModel
    {
        public int RepositoryCount { set; get; }

        public int ToolsCountInAllRepositories { set; get; }

        public int ToolRepoAssociationsCount { set; get; }

        public int ToolsWithNoPublications { set; get; }

        public int ToolsWithOnePublication { set; get; }

        public int ToolsWithMoreThanOnePublications { set; get; }
    }
}
