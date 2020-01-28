using Genometric.BibitemParser;
using Genometric.BibitemParser.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(PublicationJsonConverter))]
    public class Publication : IPublication<Author, Keyword>
    {
        public int ID { set; get; }

        public int ToolID { set; get; }

        public string PubMedID { set; get; }

        public string EID { set; get; }

        public string ScopusID { set; get; }

        public BibTexEntryType Type { set; get; }

        public string Title { set; get; }

        public int? Year { set; get; }

        public int? Month { set; get; }

        public int? Day { set; get; }

        public int? CitedBy { set; get; }

        public string DOI { set; get; }

        public string BibTeXEntry { set; get; }

        public string Journal { set; get; }

        public string Volume { set; get; }

        public int? Number { set; get; }

        public string Chapter { set; get; }

        public string Pages { set; get; }

        public string Publisher { set; get; }

        public virtual Tool Tool { set; get; }

        public virtual ICollection<Citation> Citations { set; get; }

        public virtual ICollection<Author> Authors { set; get; }

        public virtual ICollection<Keyword> Keywords { set; get; }

        public virtual ICollection<AuthorPublication> AuthorPublications { set; get; }

        public Publication() { }

        public Publication(
            string pubMedID = default,
            string eID = default,
            string scopusID = default,
            BibTexEntryType type = default,
            string title = default,
            int? year = default,
            int? month = default,
            int citedBy = default,
            string doi = default,
            string bibTeXEntry = default,
            string journal = default,
            string volume = default,
            int? number = default,
            string chapter = default,
            string pages = default,
            string publisher = default,
            Tool tool = default,
            ICollection<Citation> citations = default,
            List<Author> authors = default,
            List<Keyword> keywords = default)
        {
            PubMedID = pubMedID;
            EID = eID;
            ScopusID = scopusID;
            Type = type;
            Title = title;
            Year = year;
            Month = month;
            CitedBy = citedBy;
            DOI = doi;
            BibTeXEntry = bibTeXEntry;
            Journal = journal;
            Volume = volume;
            Number = number;
            Chapter = chapter;
            Pages = pages;
            Publisher = publisher;
            Tool = tool;
            Citations = citations;
            Authors = authors;
            Keywords = keywords;
        }
    }
}
