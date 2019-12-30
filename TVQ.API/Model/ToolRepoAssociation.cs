using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(ToolRepoAssociationJsonConverter))]
    public class ToolRepoAssociation
    {
        public int ID { set; get; }

        public string IDinRepo { set; get; }

        public int ToolID { set; get; }

        public int RepositoryID { set; get; }

        public string UserID { set; get; }

        public int? TimesDownloaded { set; get; }

        public ICollection<ToolDownloadRecord> Downloads { set; get; }

        public DateTime? DateAddedToRepository { set; get; }

        public Tool Tool { set; get; }

        public Repository Repository { set; get; }

        public ToolRepoAssociation()
        {
            Downloads = new List<ToolDownloadRecord>();
        }

        public ToolRepoAssociation(RepoTool repoTool) : this()
        {
            IDinRepo = repoTool.IDinRepo;
            UserID = repoTool.UserID;
            TimesDownloaded = repoTool.TimesDownloaded;
            DateAddedToRepository = repoTool.DateAddedToRepository;
            Tool = new Tool()
            {
                Name = repoTool.Name,
                Homepage = repoTool.Homepage,
                CodeRepo = repoTool.CodeRepo,
                Owner = repoTool.Owner,
                Description = repoTool.Description
            };
        }
    }
}
