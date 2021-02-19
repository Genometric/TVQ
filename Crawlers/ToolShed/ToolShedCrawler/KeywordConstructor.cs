using Genometric.BibitemParser.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ToolShedCrawler
{
    public class KeywordConstructor : IKeywordConstructor<Keyword>
    {
        public Keyword Construct(string label)
        {
            return new Keyword(label);
        }
    }
}
