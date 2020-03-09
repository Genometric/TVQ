using Genometric.TVQ.API.Model.Associations;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(ToolJsonConverter))]
    public class Tool : BaseModel
    {
        public string Name { set; get; }

        public string Homepage { set; get; }

        public string CodeRepo { set; get; }

        public string Owner { set; get; }

        public string Description { set; get; }

        public virtual ICollection<ToolRepoAssociation> RepoAssociations { set; get; }

        public virtual ICollection<ToolCategoryAssociation> CategoryAssociations { set; get; }

        public virtual ICollection<ToolPublicationAssociation> PublicationAssociations { set; get; }

        public Tool()
        {
            RepoAssociations = new List<ToolRepoAssociation>();
            CategoryAssociations = new List<ToolCategoryAssociation>();
            PublicationAssociations = new List<ToolPublicationAssociation>();
        }
    }
}
