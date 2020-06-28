using Genometric.TVQ.API.Model.JsonConverters;
using Newtonsoft.Json;

namespace Genometric.TVQ.API.Model
{
    public enum State { Queued = 0, Running = 1, Completed = 2, Failed = 3 };

    [JsonConverter(typeof(BaseJsonConverter))]
    public abstract class BaseJob : BaseModel
    {
        public State Status { set; get; }

        public string Message { set; get; }
    }
}
