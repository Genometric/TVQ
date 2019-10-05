using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(PublicationJsonConverter))]
    public class Publication
    {
        public int ID { set; get; }

        public int ToolID { set; get; }

        public string PubMedID { set; get; }

        public string Title { set; get; }

        public string Year { set; get; }

        public int CitedBy { set; get; }

        public string DOI { set; get; }

        public string BibTeXEntry { set; get; }

        public virtual List<Citation> Citations { set; get; }

        public virtual Tool Tool { set; get; }

        public Publication()
        { }
    }
}
