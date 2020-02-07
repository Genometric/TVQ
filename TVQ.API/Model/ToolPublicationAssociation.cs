namespace Genometric.TVQ.API.Model
{
    public class ToolPublicationAssociation
    {
        public int ID { set; get; }

        public int ToolID { set; get; }

        public int PublicationID { set; get; }

        public virtual Tool Tool { set; get; }

        public virtual Publication Publication { set; get; }
    }
}
