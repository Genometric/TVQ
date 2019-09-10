using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(ToolJsonConverter))]
    public class Tool
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        public int RepositoryID { set; get; }

        public Repository Repo { set; get; }

        public string IDinRepo { set; get; }

        public string Name { set; get; }

        public string Homepage { set; get; }

        public string CodeRepo { set; get; }

        public string Owner { set; get; }

        public string UserID { set; get; }

        public string Description { set; get; }

        public int TimesDownloaded { set; get; }

        public Tool() { }
    }
}
