namespace Genometric.TVQ.API.Model.Associations
{
    public class ToolPublicationAssociation : BaseModel
    {
        public int ToolID { set; get; }

        public int PublicationID { set; get; }

        public virtual Tool Tool { set; get; }

        public virtual Publication Publication { set; get; }
    }
}
