using Genometric.TVQ.WebService.Model.JsonConverters;
using Newtonsoft.Json;

namespace Genometric.TVQ.WebService.Model.Associations
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class ToolCategoryAssociation : BaseModel
    {
        public int ToolID { set; get; }
        public virtual Tool Tool { set; get; }

        public int CategoryID { set; get; }
        public virtual Category Category { set; get; }
    }
}
