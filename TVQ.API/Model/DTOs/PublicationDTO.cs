using System.Collections.Generic;

namespace Genometric.TVQ.API.Model.DTOs
{
    public class PublicationDTO
    {
        public int ID { get; }

        public List<int> ToolAssociations { get; }

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
