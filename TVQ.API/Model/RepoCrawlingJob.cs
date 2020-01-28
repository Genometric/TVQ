namespace Genometric.TVQ.API.Model
{
    public class RepoCrawlingJob : BaseJob
    {
        public int RepositoryID { set; get; }
        public Repository Repository { set; get; }
    }
}
