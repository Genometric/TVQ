using Genometric.BibitemParser.Interfaces;

namespace ToolShedCrawler
{
    public class AuthorConstructor : IAuthorConstructor<Author>
    {
        public Author Construct(string firstName, string lastName)
        {
            return new Author(firstName, lastName);
        }
    }
}
