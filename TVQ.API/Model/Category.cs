using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(CategoryJsonConverter))]
    public class Category
    {
        public int ID { set; get; }

        public string ToolShedID { set; get; }

        public string Name { set; get; }

        public string Description { set; get; }

        public virtual ICollection<ToolCategoryAssociation> ToolAssociations { set; get; }

        public Category()
        {
            ToolAssociations = new List<ToolCategoryAssociation>();
        }
    }
}
