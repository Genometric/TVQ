using Genometric.TVQ.API.Model.Associations;
using Genometric.TVQ.API.Model.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(RepositoryJsonConverter))]
    public class Repository
    {
        public enum Repo { ToolShed, BioTools, Bioconductor, Bioconda };

        public int ID { set; get; }

        public Repo? Name { set; get; }

        public string URI { set; get; }

        public int ToolsCount
        {
            get
            {
                if (ToolAssociations != null)
                    return ToolAssociations.Count;
                else
                    return 0;
            }
        }

        public virtual ICollection<ToolRepoAssociation> ToolAssociations { set; get; }

        public virtual Statistics Statistics { set; get; }

        public Repository() { }

        public Uri GetURI()
        {
            return new Uri(URI);
        }
    }
}
