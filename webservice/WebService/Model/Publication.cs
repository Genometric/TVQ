using Genometric.BibitemParser;
using Genometric.TVQ.WebService.Crawlers.ToolRepos.HelperTypes;
using Genometric.TVQ.WebService.Model.Associations;
using Genometric.TVQ.WebService.Model.JsonConverters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genometric.TVQ.WebService.Model
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class Publication : BaseModel
    {
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

        public virtual ICollection<Citation> Citations { set; get; }

        public virtual ICollection<AuthorPublicationAssociation> AuthorAssociations { set; get; }

        public virtual ICollection<ToolPublicationAssociation> ToolAssociations { set; get; }

        public virtual ICollection<PublicationKeywordAssociation> KeywordAssociations { set; get; }

        public Publication()
        {
            Citations = new List<Citation>();
            ToolAssociations = new List<ToolPublicationAssociation>();
            AuthorAssociations = new List<AuthorPublicationAssociation>();
            KeywordAssociations = new List<PublicationKeywordAssociation>();
        }

        public Publication(ParsedPublication parsedPublication) : this()
        {
            PubMedID = parsedPublication.PubMedID;
            EID = parsedPublication.EID;
            ScopusID = parsedPublication.ScopusID;
            Type = parsedPublication.Type;
            Title = parsedPublication.Title;
            Year = parsedPublication.Year;
            Month = parsedPublication.Month;
            Day = parsedPublication.Day;
            CitedBy = parsedPublication.CitedBy;
            DOI = parsedPublication.DOI;
            BibTeXEntry = parsedPublication.BibTeXEntry;
            Journal = parsedPublication.Journal;
            Volume = parsedPublication.Volume;
            Number = parsedPublication.Number;
            Chapter = parsedPublication.Chapter;
            Pages = parsedPublication.Pages;
            Publisher = parsedPublication.Publisher;
            Citations = parsedPublication.Citations;

            if (parsedPublication.Authors != null && parsedPublication.Authors.Count > 0)
            {
                AuthorAssociations = new List<AuthorPublicationAssociation>();
                foreach (var author in parsedPublication.Authors)
                    AuthorAssociations.Add(new AuthorPublicationAssociation(author, this));
            }

            if (parsedPublication.Keywords != null && parsedPublication.Keywords.Count > 0)
            {
                KeywordAssociations = new List<PublicationKeywordAssociation>();
                foreach (var keyword in parsedPublication.Keywords)
                    KeywordAssociations.Add(new PublicationKeywordAssociation(keyword, this));
            }
        }
    }
}
