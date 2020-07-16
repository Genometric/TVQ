using Genometric.TVQ.WebService.Model.JsonConverters;
using Newtonsoft.Json;

namespace Genometric.TVQ.WebService.Model
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class AnalysisJob : BaseJob
    {
        public virtual Repository Repository { set; get; }
    }
}
