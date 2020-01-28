using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(ToolJsonConverter))]
    public class Tool
    {
        public int ID { set; get; }

        public string Name { set; get; }

        public string Homepage { set; get; }

        public string CodeRepo { set; get; }

        public string Owner { set; get; }

        public string Description { set; get; }

        public virtual ICollection<ToolRepoAssociation> RepoAssociations { set; get; }

        public virtual ICollection<Publication> Publications { set; get; }

        public virtual ICollection<ToolCategoryAssociation> CategoryAssociations { set; get; }

        public Tool()
        {
            RepoAssociations = new List<ToolRepoAssociation>();
            Publications = new List<Publication>();
            CategoryAssociations = new List<ToolCategoryAssociation>();
        }
    }
}
