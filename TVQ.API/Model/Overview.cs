using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Model
{
    public class Overview
    {
        public int ID { set; get; }

        public int RepositoryCount { set; get; }

        public int ToolsCount { set; get; }

        public int ToolRepoAssociationsCount { set; get; }

        public int ToolsWithNoPublications { set; get; }

        public int ToolsWithOnePublication { set; get; }

        public int ToolsWithMoreThanOnePublications { set; get; }
    }
}
