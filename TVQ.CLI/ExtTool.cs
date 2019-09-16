using Genometric.TVQ.API.Model;
using System.Collections.Generic;

namespace Genometric.TVQ.CLI
{
    public class ExtTool : Tool
    {
        public List<Publication> Publications { set; get; }

        public ExtTool() { }

        public ExtTool(Tool tool)
        {
            Id = tool.Id;
            RepositoryID = tool.RepositoryID;
            Repo = tool.Repo;
            PublicationID = tool.PublicationID;
            Pub = tool.Pub;
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
