using Genometric.TVQ.API.Model.JsonConverters;
using Newtonsoft.Json;

namespace Genometric.TVQ.API.Model.Associations
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class ToolPublicationAssociation : BaseModel
    {
        public int ToolID { set; get; }

        public int PublicationID { set; get; }

        public virtual Tool Tool { set; get; }

        public virtual Publication Publication { set; get; }
    }
}
