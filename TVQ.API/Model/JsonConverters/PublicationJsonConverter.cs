using System.Collections.Generic;

namespace Genometric.TVQ.API.Model.JsonConverters
{
    public class PublicationJsonConverter : BaseJsonConverter
    {
        public PublicationJsonConverter() : base(
            propertyMappings: new Dictionary<string, string>
            {
                {"id", nameof(Publication.ID)},
                {"title", nameof(Publication.Title)},
                {"year", nameof(Publication.Year)},
                {"CitedBy", nameof(Publication.CitedBy)},
                {"doi", nameof(Publication.DOI)},
                {"citation", nameof(Publication.BibTeXEntry)},
                {"pmid", nameof(Publication.PubMedID)}
            })
        { }
    }
}
