using Genometric.BibitemParser.Interfaces;

namespace Genometric.TVQ.Crawlers.ToolShedCrawler
{
    public class Keyword: IKeyword
    {
        public string Label { get; }

        public Keyword(string label)
        {
            Label = label;
        }
    }
}
