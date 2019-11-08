using Genometric.BibitemParser.Interfaces;

namespace Genometric.TVQ.API.Model
{
    public class Author : IAuthor
    {
        public int ID { set; get; }

        public int PublicationID { set; get; }

        public string FirstName { get; }

        public string LastName { get; }

        public Publication Publication { set; get; }

        public Author(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
