using Genometric.BibitemParser.Interfaces;
using Genometric.TVQ.API.Model.Associations;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
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
