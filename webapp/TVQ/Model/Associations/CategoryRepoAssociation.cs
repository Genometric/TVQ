using Genometric.TVQ.API.Model.JsonConverters;
using Newtonsoft.Json;

namespace Genometric.TVQ.API.Model.Associations
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class CategoryRepoAssociation : BaseModel
    {
        public string IDinRepo { set; get; }

        public int CategoryID { set; get; }

        public virtual Category Category { set; get; }

        public int RepositoryID { set; get; }

        public virtual Repository Repository { set; get; }
    }
}
