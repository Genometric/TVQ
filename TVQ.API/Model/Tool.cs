using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(ToolJsonConverter))]
    public class Tool
    {
        public int ID { set; get; }

        public int RepositoryID { set; get; }

        public string IDinRepo { set; get; }

        public string Name { set; get; }

        public string Homepage { set; get; }

        public string CodeRepo { set; get; }

        public string Owner { set; get; }

        public string UserID { set; get; }

        public string Description { set; get; }

        public int TimesDownloaded { set; get; }

        public virtual Repository Repository { set; get; }

        public virtual List<Publication> Publications { set; get; }

        public Tool() { }
    }
}
