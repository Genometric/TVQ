namespace Genometric.TVQ.API.Model.Associations
{
    public class AuthorPublicationAssociation
    {
        public int ID { set; get; }

        public int AuthorID { set; get; }
        public virtual Author Author { set; get; }

        public int PublicationID { set; get; }
        public virtual Publication Publication { set; get; }

        // This parameterless constructor is required by EF.
        public AuthorPublicationAssociation() { }

        public AuthorPublicationAssociation(Author author, Publication publication)
        {
            Author = author;
            Publication = publication;
        }
    }
}
