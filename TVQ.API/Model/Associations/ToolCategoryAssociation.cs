namespace Genometric.TVQ.API.Model.Associations
{
    public class ToolCategoryAssociation : BaseModel
    {
        public int ToolID { set; get; }
        public virtual Tool Tool { set; get; }

        public int CategoryID { set; get; }
        public virtual Category Category { set; get; }
    }
}
