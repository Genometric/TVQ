using Genometric.TVQ.API.Model.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model.Associations
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class ToolRepoAssociation : BaseModel
    {
        public string IDinRepo { set; get; }

        public int ToolID { set; get; }

        public int RepositoryID { set; get; }

        public string Owner { set; get; }

        public string UserID { set; get; }

        public int? TimesDownloaded { set; get; }

        public virtual ICollection<ToolDownloadRecord> Downloads { set; get; }

        public DateTime? DateAddedToRepository { set; get; }

        public virtual Tool Tool { set; get; }

        public virtual Repository Repository { set; get; }

        public ToolRepoAssociation()
        {
            Downloads = new List<ToolDownloadRecord>();
        }
    }
}
