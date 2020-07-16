using Genometric.TVQ.WebService.Model.JsonConverters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genometric.TVQ.WebService.Model
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class LiteratureCrawlingJob : BaseJob
    {
        public bool ScanAllPublications { set; get; } = false;

        // TODO: this should not be correct! 
        // This is a many-to-many relation and should not be implemented this.
        public virtual List<Publication> Publications { set; get; }
    }
}
