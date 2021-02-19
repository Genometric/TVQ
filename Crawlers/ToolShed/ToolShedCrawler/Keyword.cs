using Genometric.BibitemParser.Interfaces;

namespace ToolShedCrawler
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
