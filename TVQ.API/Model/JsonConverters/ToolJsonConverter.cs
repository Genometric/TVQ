using System.Collections.Generic;

namespace Genometric.TVQ.API.Model.JsonConverters
{
    public class ToolJsonConverter : BaseJsonConverter
    {
        public ToolJsonConverter() : base(
            propertyMappings: new Dictionary<string, string>
            {
                {"name", nameof(Category.Name)},
                {"homepage", nameof(Tool.Homepage)},
                {"homepage_url", nameof(Tool.Homepage)},
                {"owner", nameof(Tool.Owner)},
                {"remote_repository_url", nameof(Tool.CodeRepo)},
                {"description", nameof(Tool.Description)}
            })
        { }
    }
}
