using Genometric.TVQ.WebService.Model.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Genometric.TVQ.WebService.Model.DTOs
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class PublicationDTO : BaseModel
    {
        public List<int> ToolAssociations { get; }

        [JsonIgnore]
        public new DateTime CreatedDate { set; get; }

        [JsonIgnore]
        public new DateTime UpdatedDate { set; get; }

        public PublicationDTO(Publication publication)
        {
            if (publication == null)
                return;

            ID = publication.ID;
            ToolAssociations = new List<int>();
            foreach (var association in publication.ToolAssociations)
                ToolAssociations.Add(association.ID);
        }
    }
}
