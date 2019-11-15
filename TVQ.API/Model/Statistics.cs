namespace Genometric.TVQ.API.Model
{
    public class Statistics
    {
        public int ID { set; get; }

        public int RepositoryID { set; get; }

        public Repository Repository { set; get; }

        public double? TValue { set; get; }
    }
}
