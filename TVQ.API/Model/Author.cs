using Genometric.BibitemParser.Interfaces;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    public class Author : IAuthor
    {
        public int ID { set; get; }

        public string FirstName { get; }

        public string LastName { get; }

        public virtual ICollection<AuthorPublicationAssociation> AuthorPublications { set; get; }

        public Author(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
