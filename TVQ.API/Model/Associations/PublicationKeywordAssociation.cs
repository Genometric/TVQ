namespace Genometric.TVQ.API.Model.Associations
{
    public class PublicationKeywordAssociation : BaseModel
    {
        public int PublicationID { set; get; }
        public virtual Publication Publication { set; get; }

        public int KeywordID { set; get; }
        public virtual Keyword Keyword { set; get; }

        public PublicationKeywordAssociation() { }

        public PublicationKeywordAssociation(Keyword keyword, Publication publication)
        {
            Keyword = keyword;
            Publication = publication;
        }
    }
}
