namespace Genometric.TVQ.API.Model
{
    public class PublicationDTO
    {
        public int ID { get; }

        public int ToolID { get; }

        public PublicationDTO(Publication publication)
        {
            if (publication == null)
                return;

            ID = publication.ID;
            ToolID = publication.ToolID;
        }
    }
}
