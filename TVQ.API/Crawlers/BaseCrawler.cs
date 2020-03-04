using Genometric.TVQ.API.Model;
using Genometric.TVQ.API.Model.Associations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Crawlers
{
    public abstract class BaseCrawler : IDisposable
    {
        protected WebClient WebClient { get; }

        protected string SessionTempPath { get; }

        public ReadOnlyCollection<string> TitleStopWords
        {
            get { return Array.AsReadOnly(_titleStopWords); }
        }
        private readonly string[] _titleStopWords = new string[]
        {
            "{", "}",

            // Scopus fails to find the article when its title contains `?`. 
            "?",

            // Open and close parenthesis are Scopus query segment delimiters;
            // hence, having them as part of a publication title will a malformed
            // request (i.e., BAD REQUEST). They cannot be escaped, doing so 
            // will cause Scopus fail to find a match.
            "(", ")",

            // Some publication titles may have LaTeX formatting, it is better
            // to remove them more appropriately. Meanwhile, removing the following
            // strings seems reasonable. 
            "\\it"
        };

        protected List<Publication> Publications { get; }

        private ConcurrentDictionary<string, Publication> PubsByDOI { get; }
        private ConcurrentDictionary<string, Publication> PubsByPMID { get; }
        private ConcurrentDictionary<string, Publication> PubsByTitle { get; }

        private ConcurrentDictionary<string, ToolPublicationAssociation> ToolPubAssoByDOI { get; }
        private ConcurrentDictionary<string, ToolPublicationAssociation> ToolPubAssoByPMID { get; }
        private ConcurrentDictionary<string, ToolPublicationAssociation> ToolPubAssoByTitle { get; }

        public List<Publication> PublicationsToBeDeleted { get; }

        protected BaseCrawler(List<Publication> publications)
        {
            WebClient = new WebClient();

            do
            {
                SessionTempPath =
                    Path.GetFullPath(Path.GetTempPath()) +
                    Utilities.GetRandomString(10) +
                    Path.DirectorySeparatorChar;
            }
            while (Directory.Exists(SessionTempPath));
            Directory.CreateDirectory(SessionTempPath);

            Publications = publications;
            PubsByDOI = new ConcurrentDictionary<string, Publication>();
            PubsByPMID = new ConcurrentDictionary<string, Publication>();
            PubsByTitle = new ConcurrentDictionary<string, Publication>();
            ToolPubAssoByDOI = new ConcurrentDictionary<string, ToolPublicationAssociation>();
            ToolPubAssoByPMID = new ConcurrentDictionary<string, ToolPublicationAssociation>();
            ToolPubAssoByTitle = new ConcurrentDictionary<string, ToolPublicationAssociation>();
            InitializePublicationHelper();

            PublicationsToBeDeleted = new List<Publication>();
        }

        private static string FormatToolName(string toolName)
        {
            return toolName.ToUpperInvariant();
        }

        public string FormatPublicationTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                return title;

            foreach (var word in _titleStopWords)
                title = title.Replace(word, string.Empty, StringComparison.InvariantCultureIgnoreCase);

            return title;
        }

        private string GetAssociationHashKey(string toolName, string attribute)
        {
            return FormatToolName(toolName) + "::" + attribute;
        }

        private void InitializePublicationHelper()
        {
            if (Publications == null || Publications.Count == 0)
                return;

            foreach (var pub in Publications)
            {
                TryAddPublication(pub);
                foreach (var association in pub.ToolAssociations)
                    TryAddToolPublicationAssociation(association.Tool, association);
            }
        }

        public Task DownloadFileAsync(Uri address, out string filename)
        {
            filename = SessionTempPath + Utilities.GetRandomString();
            return WebClient.DownloadFileTaskAsync(address, filename);
        }

        public bool PublicationExists(Publication publication)
        {
            if (publication == null)
                return false;

            if (publication.DOI != null &&
                PubsByDOI.ContainsKey(publication.DOI))
                return true;

            if (publication.PubMedID != null &&
                PubsByPMID.ContainsKey(publication.PubMedID))
                return true;

            if (publication.Title != null &&
                PubsByTitle.ContainsKey(publication.Title))
                return true;

            return false;
        }

        public bool TryAddPublication(Publication publication)
        {
            if (publication == null || PublicationExists(publication))
                return false;

            if (publication.DOI != null)
                PubsByDOI.TryAdd(publication.DOI, publication);

            if (publication.PubMedID != null)
                PubsByPMID.TryAdd(publication.PubMedID, publication);

            if (publication.Title != null)
                PubsByTitle.TryAdd(publication.Title, publication);

            return true;
        }

        public bool TryRemovePublication(Publication publication)
        {
            if (publication == null)
                return false;

            if (publication.DOI != null)
                PubsByDOI.TryRemove(publication.DOI, out _);

            if (publication.PubMedID != null)
                PubsByPMID.TryRemove(publication.PubMedID, out _);

            if (publication.Title != null)
                PubsByTitle.TryRemove(publication.Title, out _);

            PublicationsToBeDeleted.Add(publication);

            return true;
        }

        public Publication GetPublication(Publication publication)
        {
            if (publication == null)
                return null;

            if (publication.DOI != null && 
                PubsByDOI.ContainsKey(publication.DOI))
                return PubsByDOI[publication.DOI];

            if (publication.PubMedID != null &&
                PubsByPMID.ContainsKey(publication.PubMedID))
                return PubsByPMID[publication.PubMedID];

            if (publication.Title != null &&
                PubsByTitle.ContainsKey(publication.Title))
                return PubsByTitle[publication.Title];

            return null;
        }

        public bool ToolPublicationAssociationExists(Tool tool, ToolPublicationAssociation association)
        {
            if (tool == null ||
                tool.Name == null ||
                association == null ||
                association.Publication == null)
                return false;

            if (association.Publication.DOI != null &&
                ToolPubAssoByDOI.ContainsKey(GetAssociationHashKey(tool.Name, association.Publication.DOI)))
                return true;

            if (association.Publication.PubMedID != null &&
                ToolPubAssoByPMID.ContainsKey(GetAssociationHashKey(tool.Name, association.Publication.PubMedID)))
                return true;

            if (association.Publication.Title != null &&
                ToolPubAssoByTitle.ContainsKey(GetAssociationHashKey(tool.Name, association.Publication.Title)))
                return true;

            return false;
        }

        public bool TryAddToolPublicationAssociation(Tool tool, ToolPublicationAssociation association)
        {
            if (tool == null || 
                association == null ||
                association.Publication == null ||
                ToolPublicationAssociationExists(tool, association))
                return false;

            if (association.Publication.DOI != null)
                ToolPubAssoByDOI.TryAdd(
                    GetAssociationHashKey(tool.Name, association.Publication.DOI),
                    association);

            if (association.Publication.Title != null)
                ToolPubAssoByTitle.TryAdd(
                    GetAssociationHashKey(tool.Name, association.Publication.Title),
                    association);

            if (association.Publication.PubMedID != null)
                ToolPubAssoByPMID.TryAdd(
                    GetAssociationHashKey(tool.Name, association.Publication.PubMedID),
                    association);

            return true;
        }

        public Publication MergePubsIfNecessary(Publication publication)
        {
            if (publication == null)
                return null;

            if ((publication.DOI != null && PubsByDOI.TryGetValue(publication.DOI, out Publication duplicate) ||
                (publication.PubMedID != null && PubsByPMID.TryGetValue(publication.PubMedID, out duplicate)) ||
                (publication.Title != null && PubsByTitle.TryGetValue(publication.Title, out duplicate))) &&
                duplicate.ID != publication.ID)
            {
                // Duplicate found.
                foreach (var association in duplicate.ToolAssociations)
                    publication.ToolAssociations.Add(
                        new ToolPublicationAssociation()
                        {
                            Publication = publication,
                            Tool = association.Tool
                        });
                
                TryRemovePublication(duplicate);
                TryAddPublication(publication);
                return publication;
            }
            else
            {
                // No duplicate.u
                return publication;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Directory.Delete(SessionTempPath, true);
        }
    }
}
