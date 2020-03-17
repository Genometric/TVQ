using Genometric.TVQ.API.Model.JsonConverters;
using Newtonsoft.Json;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class RepoCrawlingJob : BaseJob
    {
        public int RepositoryID { set; get; }

        public virtual Repository Repository { set; get; }
    }
}
