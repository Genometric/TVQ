using System.Collections.Generic;

namespace Genometric.TVQ.API.Model.JsonConverters
{
    public class RepositoryJsonConverter : BaseJsonConverter
    {
        public RepositoryJsonConverter() : base(
            propertyMappings: new Dictionary<string, string>
            {
                { "id", nameof(Repository.ID) },
                { "name", nameof(Repository.Name) },
                { "uri", nameof(Repository.URI) }
            },
            propertiesToIgnore: new List<string>
            {
                nameof(Repository.ToolAssociations)
            })
        { }
    }
}
