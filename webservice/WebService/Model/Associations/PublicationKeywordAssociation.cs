using Genometric.TVQ.WebService.Model.JsonConverters;
using Newtonsoft.Json;

namespace Genometric.TVQ.WebService.Model.Associations
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class PublicationKeywordAssociation : BaseModel
    {
        public int PublicationID { set; get; }
        public virtual Publication Publication { set; get; }

        public int KeywordID { set; get; }
        public virtual Keyword Keyword { set; get; }

        public PublicationKeywordAssociation() { }

        public PublicationKeywordAssociation(Keyword keyword, Publication publication)
        {
            Keyword = keyword;
            Publication = publication;
        }
    }
}
