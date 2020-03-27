using Genometric.TVQ.API.Model.Associations;
using Genometric.TVQ.API.Model.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class Tool : BaseModel, IEquatable<Tool>
    {
        public string Name { set; get; }

        public string Homepage { set; get; }

        public string CodeRepo { set; get; }

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

        public bool Equals([AllowNull] Tool other)
        {
            return ID == other?.ID;
        }

        public override string ToString()
        {
            return ID + "\t" + Name;
        }
    }
}
