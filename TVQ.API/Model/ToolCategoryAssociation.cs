namespace Genometric.TVQ.API.Model
{
    public class ToolCategoryAssociation
    {
        public int ID { set; get; }

        public int ToolID { set; get; }
        public virtual Tool Tool { set; get; }

        public int CategoryID { set; get; }
        public virtual Category Category { set; get; }
    }
}
