using Genometric.BibitemParser.Interfaces;

namespace Genometric.TVQ.API.Model
{
    public class KeywordConstructor : IKeywordConstructor<Keyword>
    {
        public Keyword Construct(string label)
        {
            return new Keyword(label);
        }
    }
}
