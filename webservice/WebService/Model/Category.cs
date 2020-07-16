using Genometric.TVQ.WebService.Model.Associations;
using Genometric.TVQ.WebService.Model.JsonConverters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genometric.TVQ.WebService.Model
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class Category : BaseModel
    {
        public string Name { set; get; }

        public string Description { set; get; }

        public string URI { set; get; }

        public virtual ICollection<ToolCategoryAssociation> ToolAssociations { set; get; }

        public virtual ICollection<CategoryRepoAssociation> RepoAssociations { set; get; }

        public Category()
        {
            ToolAssociations = new List<ToolCategoryAssociation>();
        }
    }
}
