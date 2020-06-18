using Genometric.TVQ.API.Model.Associations;
using Genometric.TVQ.API.Model.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class Repository : BaseModel
    {
        public enum Repo { ToolShed, BioTools, Bioconductor, Bioconda };

        public Repo? Name { set; get; }

        public string URI { set; get; }

        [JsonIgnore]
        public virtual ICollection<ToolRepoAssociation> ToolAssociations { set; get; }

        [JsonIgnore]
        public virtual ICollection<CategoryRepoAssociation> CategoryAssociations { set; get; }

        public virtual Statistics Statistics { set; get; }

        public Repository() { }

        public Uri GetURI()
        {
            return new Uri(URI);
        }
    }
}
