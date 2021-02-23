using Genometric.BibitemParser.Interfaces;

namespace Genometric.TVQ.Crawlers.ToolShedCrawler
{
    public class KeywordConstructor : IKeywordConstructor<Keyword>
    {
        public Keyword Construct(string label)
        {
            return new Keyword(label);
        }
    }
}
