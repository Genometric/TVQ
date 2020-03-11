using Genometric.TVQ.API.Model.Associations;
using Genometric.TVQ.API.Model.JsonConverters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class Category : BaseModel
    {
        public string ToolShedID { set; get; }

        public string Name { set; get; }

        public string Description { set; get; }

        public string URI { set; get; }

        public virtual ICollection<ToolCategoryAssociation> ToolAssociations { set; get; }

        public Category()
        {
            ToolAssociations = new List<ToolCategoryAssociation>();
        }
    }
}
