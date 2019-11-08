using Genometric.BibitemParser.Interfaces;

namespace Genometric.TVQ.API.Model
{
    public class Keyword : IKeyword
    {
        public int ID { set; get; }

        public int PublicationID { set; get; }

        public string Label { get; }

        public Publication Publication { set; get; }

        public Keyword(string label)
        {
            Label = label;
        }
    }
}
