using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(RepositoryJsonConverter))]
    public class Repository : BaseModel
    {
        public enum Repo { ToolShed, BioTools, Bioconductor };

        public int ID { set; get; }

        public Repo? Name { set; get; }

        public string URI { set; get; }

        public int ToolsCount
        {
            get
            {
                if (Tools != null)
                    return Tools.Count;
                else
                    return 0;
            }
        }

        public ICollection<Tool> Tools { set; get; }

        public Statistics Statistics { set; get; }

        public Repository() { }

        public Uri GetURI()
        {
            return new Uri(URI);
        }
    }
}
