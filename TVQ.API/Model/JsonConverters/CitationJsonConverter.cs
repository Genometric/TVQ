using System.Collections.Generic;

namespace Genometric.TVQ.API.Model.JsonConverters
{
    public class CitationJsonConverter : BaseJsonConverter
    {
        public CitationJsonConverter() : base(
            new Dictionary<string, string>
            {
                {"id", nameof(Citation.ID) },
                {"publication_id", nameof(Citation.PublicationID) },
                {"count", nameof(Citation.Count) },
                {"accumulated_count", nameof(Citation.AccumulatedCount) },
                {"date", nameof(Citation.Date) },
                {"source", nameof(Citation.Source) },
                {"publication", nameof(Citation.Publication) }
            })
        { }
    }
}
