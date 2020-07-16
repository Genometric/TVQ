using Genometric.TVQ.WebService.Model.JsonConverters;
using Newtonsoft.Json;

namespace Genometric.TVQ.WebService.Model.Associations
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class AuthorPublicationAssociation : BaseModel
    {
        public int AuthorID { set; get; }
        public virtual Author Author { set; get; }

        public int PublicationID { set; get; }
        public virtual Publication Publication { set; get; }

        // This parameterless constructor is required by EF.
        public AuthorPublicationAssociation() { }

        public AuthorPublicationAssociation(Author author, Publication publication)
        {
            Author = author;
            Publication = publication;
        }
    }
}
