using Genometric.BibitemParser.Interfaces;
using Genometric.TVQ.API.Model.Associations;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    public class Keyword : IKeyword
    {
        public int ID { set; get; }

        public string Label { get; }

        public virtual ICollection<PublicationKeywordAssociation> PublicationAssociations { set; get; }

        public Keyword(string label)
        {
            Label = label;
        }
    }
}
