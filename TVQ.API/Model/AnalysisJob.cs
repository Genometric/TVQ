using Genometric.TVQ.API.Model.JsonConverters;
using Newtonsoft.Json;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class AnalysisJob : BaseJob
    {
        public virtual Repository Repository { set; get; }
    }
}
