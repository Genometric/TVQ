using Genometric.BibitemParser;
using Genometric.BibitemParser.Interfaces;
using Genometric.TVQ.API.Model;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Crawlers.ToolRepos.HelperTypes
{
    public class ParsedPublication : Publication, IPublication<Author, Keyword>
    {
        public ICollection<Author> Authors { set; get; }

        public ICollection<Keyword> Keywords { set; get; }

        public ParsedPublication(
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
            ICollection<Citation> citations = default,
            List<Author> authors = default,
            List<Keyword> keywords = default,
            ICollection<ToolPublicationAssociation> toolAssociations = default)
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
            Citations = citations;
            Authors = authors;
            Keywords = keywords;
            ToolAssociations = toolAssociations;
        }
    }
}
