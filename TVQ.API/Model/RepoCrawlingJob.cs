namespace Genometric.TVQ.API.Model
{
    public class RepoCrawlingJob : BaseJob
    {
        public int RepositoryID { set; get; }

        public virtual Repository Repository { set; get; }
    }
}
