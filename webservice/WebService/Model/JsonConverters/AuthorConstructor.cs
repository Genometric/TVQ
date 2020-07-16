using Genometric.BibitemParser.Interfaces;

namespace Genometric.TVQ.WebService.Model.JsonConverters
{
    public class AuthorConstructor : IAuthorConstructor<Author>
    {
        public Author Construct(string firstName, string lastName)
        {
            return new Author(firstName, lastName);
        }
    }
}
