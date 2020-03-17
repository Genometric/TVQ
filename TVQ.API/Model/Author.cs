using Genometric.BibitemParser.Interfaces;
using Genometric.TVQ.API.Model.Associations;
using Genometric.TVQ.API.Model.JsonConverters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class Author : BaseModel, IAuthor
    {
        public string FirstName { set; get; }

        public string LastName { set; get; }

        public virtual ICollection<AuthorPublicationAssociation> AuthorPublications { set; get; }

        public Author(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
