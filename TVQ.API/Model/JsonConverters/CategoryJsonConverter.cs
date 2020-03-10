using System.Collections.Generic;

namespace Genometric.TVQ.API.Model.JsonConverters
{
    public class CategoryJsonConverter : BaseJsonConverter
    {
        public CategoryJsonConverter() : base(
            propertyMappings: new Dictionary<string, string>
            {
                {"name", nameof(Category.Name)},
                {"term", nameof(Category.Name)},
                {"id", nameof(Category.ToolShedID)},
                {"uri", nameof(Category.URI)},
                {"description", nameof(Category.Description)}
            })
        { }
    }
}
