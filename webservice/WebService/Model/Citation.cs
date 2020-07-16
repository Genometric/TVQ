using Genometric.TVQ.WebService.Model.JsonConverters;
using Newtonsoft.Json;
using System;

namespace Genometric.TVQ.WebService.Model
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class Citation : BaseModel
    {
        public enum InfoSource { Scopus };

        public int PublicationID { set; get; }

        public int Count { set; get; }

        public int AccumulatedCount { set; get; }

        public DateTime Date { set; get; }

        public InfoSource? Source { set; get; }

        public virtual Publication Publication { set; get; }

        public Citation() { }
    }
}
