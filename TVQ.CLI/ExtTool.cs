using Genometric.TVQ.API.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genometric.TVQ.CLI
{
    [JsonConverter(typeof(ExtToolJsonConverter))]
    public class ExtTool : Tool
    {
        public List<Publication> Publications { set; get; }

        public ExtTool() { }

        public ExtTool(Tool tool)
        {
            ID = tool.ID;
            RepoID = tool.RepoID;
            Repository = tool.Repository;
            base.Publications = tool.Publications;
            IDinRepo = tool.IDinRepo;
            Name = tool.Name;
            Homepage = tool.Homepage;
            CodeRepo = tool.CodeRepo;
            Owner = tool.Owner;
            UserID = tool.UserID;
            Description = tool.Description;
            TimesDownloaded = tool.TimesDownloaded;
            Publications = new List<Publication>();
        }
    }
}
