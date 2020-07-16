using Genometric.BibitemParser.Interfaces;
using Genometric.TVQ.WebService.Model.Associations;
using Genometric.TVQ.WebService.Model.JsonConverters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genometric.TVQ.WebService.Model
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class Keyword : BaseModel, IKeyword
    {
        public string Label { set; get; }

        public virtual ICollection<PublicationKeywordAssociation> PublicationAssociations { set; get; }

        public Keyword(string label)
        {
            Label = label;
        }
    }
}
