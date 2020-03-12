using Genometric.TVQ.API.Model.JsonConverters;
using Newtonsoft.Json;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class Statistics : BaseModel
    {
        public int RepositoryID { set; get; }

        public virtual Repository Repository { set; get; }

        public double? TScore { set; get; }

        public double? PValue { set; get; }

        public double? DegreeOfFreedom { set; get; }

        public double? CriticalValue { set; get; }

        public bool? MeansSignificantlyDifferent { set; get; }
    }
}
