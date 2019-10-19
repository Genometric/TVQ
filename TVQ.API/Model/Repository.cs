using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(RepositoryJsonConverter))]
    public class Repository
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

        public virtual List<Tool> Tools { get; }

        public Repository()
        {
            Tools = new List<Tool>();
        }

        public Uri GetURI()
        {
            return new Uri(URI);
        }
    }
}
