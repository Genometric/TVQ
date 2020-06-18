using Genometric.TVQ.API.Model.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Diagnostics.Contracts;

namespace Genometric.TVQ.API.Model.DTOs
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class ToolDTO : BaseModel
    {
        public string Name { get; }

        [JsonIgnore]
        public new DateTime CreatedDate { set; get; }

        [JsonIgnore]
        public new DateTime UpdatedDate { set; get; }

        public ToolDTO(Tool tool)
        {
            Contract.Requires(tool != null);

            ID = tool.ID;
            Name = tool.Name;
        }
    }
}
