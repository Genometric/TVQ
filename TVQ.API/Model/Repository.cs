using System;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    public class Repository
    {
        public enum Repo { ToolShed, BioTools };

        public int ID { set; get; }

        public Repo? Name { set; get; }

        public string URI { set; get; }

        public int ToolCount { set; get; }

        public virtual List<Tool> Tools { set; get; }

        public Repository() { }

        public Uri GetURI()
        {
            return new Uri(URI);
        }
    }
}
