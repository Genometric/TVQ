namespace Genometric.TVQ.API.Model
{
    public class Statistics : BaseModel
    {
        public int ID { set; get; }

        public int RepositoryID { set; get; }

        public Repository Repository { set; get; }

        public double? TScore { set; get; }

        public double? PValue { set; get; }
    }
}
