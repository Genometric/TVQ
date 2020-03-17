using Genometric.TVQ.API.Model.JsonConverters;
using Newtonsoft.Json;
using System;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class ToolDownloadRecord : BaseModel
    {
        public int ToolID { set; get; }

        public int Count { set; get; }

        public DateTime Date { set; get; }

        public virtual Tool Tool { set; get; }

        public ToolDownloadRecord() { }
    }
}
