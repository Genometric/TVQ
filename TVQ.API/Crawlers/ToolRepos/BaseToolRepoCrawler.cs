using Genometric.BibitemParser;
using Genometric.TVQ.API.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Crawlers.ToolRepos
{
    public abstract class BaseToolRepoCrawler : BaseCrawler
    {
        private readonly Parser<Publication, Author, Keyword> _bibitemParser;

        protected ConcurrentDictionary<string, Tool> ToolsDict { get; }

        protected ConcurrentDictionary<string, ToolRepoAssociation> ToolRepoAssociationsDict { get; }

        protected ConcurrentDictionary<string, Publication> PublicationsDict { get; }

        protected ConcurrentDictionary<string, ToolPublicationAssociation> ToolPubAssociationsDict { get; }

        protected Dictionary<string, Category> Categories { get; }

        public ReadOnlyCollection<Tool> Tools
        {
            get
            {
                return new ReadOnlyCollection<Tool>(ToolsDict.Values.ToList());
            }
        }

        public ConcurrentBag<ToolDownloadRecord> ToolDownloadRecords { get; }

        protected Repository Repo { get; }

        protected BaseToolRepoCrawler(
            Repository repo,
            List<Tool> tools,
            List<Publication> publications,
            List<Category> categories)
        {
            Repo = repo;
            if (tools != null)
                ToolsDict = new ConcurrentDictionary<string, Tool>(
                            tools.ToDictionary(
                                x => FormatToolName(x.Name),
                                x => x));

            if (Repo != null)
                ToolRepoAssociationsDict =
                    new ConcurrentDictionary<string, ToolRepoAssociation>(
                        repo.ToolAssociations.ToDictionary(
                            x => FormatToolRepoAssociationName(x),
                            x => x));

            if (publications != null)
            {
                PublicationsDict = new ConcurrentDictionary<string, Publication>();
                foreach (var publication in publications)
                    if (TryGetPublicationHashkey(publication, out string hashKey))
                        PublicationsDict.TryAdd(hashKey, publication);

                ToolPubAssociationsDict =
                    new ConcurrentDictionary<string, ToolPublicationAssociation>();

                foreach (var publication in publications)
                    foreach (var association in publication.ToolAssociations)
                        if (TryGetToolPubAssociationHashKey(association.Tool, association, out string hashKey))
                            ToolPubAssociationsDict.TryAdd(hashKey, association);
            }


            Categories = new Dictionary<string, Category>();
            UpdateCategories(categories);

            ToolDownloadRecords = new ConcurrentBag<ToolDownloadRecord>();

            _bibitemParser = new Parser<Publication, Author, Keyword>(
                new PublicationConstructor(),
                new AuthorConstructor(),
                new KeywordConstructor());
        }

        protected static string FormatToolName(string name)
        {
            return name.Trim().ToUpperInvariant();
        }

        private string FormatToolRepoAssociationName(Tool tool)
        {
            return Repo.Name + "::" + FormatToolName(tool.Name); ;
        }

        private string FormatToolRepoAssociationName(ToolRepoAssociation association)
        {
            return FormatToolRepoAssociationName(association.Tool);
        }

        private bool TryGetPublicationHashkey(Publication publication, out string hashKey)
        {
            if (publication.DOI != null)
                hashKey = publication.DOI;
            else if (publication.PubMedID != null)
                hashKey = publication.PubMedID;
            else if (publication.Title != null)
                hashKey = publication.Title;
            else
            {
                hashKey = null;
                return false;
            }
            return true;
        }

        private bool TryGetToolPubAssociationHashKey(
            Tool tool,
            string publicationHashKey,
            out string hashKey)
        {
            if (tool == null || tool.Name == null || publicationHashKey == null)
            {
                hashKey = null;
                return false;
            }

            hashKey = FormatToolName(tool.Name) + "::" + publicationHashKey;
            return true;
        }

        private bool TryGetToolPubAssociationHashKey(
            Tool tool,
            ToolPublicationAssociation association,
            out string hashKey)
        {
            // The reason that the "Tool" property of association is not used here, is 
            // because that property is only set if it can be set; e.g., if checking the 
            // related dictionaries, it turns out that the given association already 
            // exists, then it will not be added to the tool.

            hashKey = null;
            if (association == null ||
                association.Publication == null)
                return false;

            return
                TryGetPublicationHashkey(association.Publication, out string pubHashKey) &&
                TryGetToolPubAssociationHashKey(tool, pubHashKey, out hashKey);
        }

        public abstract Task ScanAsync();

        private void AddToolPubAssociations(Tool tool, List<ToolPublicationAssociation> associations)
        {
            if (associations == null || associations.Count == 0)
                return;

            foreach (var association in associations)
            {
                if (!TryGetPublicationHashkey(association.Publication, out string pubHashKey))
                    continue;

                // Does a publication (according to the publication hash key) as the 
                // given one has already been defined?
                // If no, then keep the parsed publication and add it to the dictionary.
                if (!PublicationsDict.TryAdd(pubHashKey, association.Publication))
                {
                    // Yes, then replaced the parsed publication with the one that already exists.
                    /// Note:
                    /// -----
                    /// Alternative to this is to merge the information in two citations. 
                    /// However, since the one already in PublicationsDict may have more complete
                    /// information because it may have been updated by the info from Scopus, this
                    /// approach is used. Also, merging two publications (fill the missing info
                    /// from each other) is not trivial.
                    association.Publication = PublicationsDict[pubHashKey];
                }

                if (TryGetToolPubAssociationHashKey(tool, pubHashKey, out string associationHashKey))
                {
                    if (ToolPubAssociationsDict.TryAdd(associationHashKey, association))
                    {
                        // The tool-publication association does not exist, and can be added.
                        association.Tool = tool;
                        tool.PublicationAssociations.Add(association);
                    }
                    else
                    {
                        // This association already exists.
                        // TODO: log this.
                        continue;
                    }
                }
                else
                {
                    // Cannot compute a hash key for the association, 
                    // TODO: log this.
                    continue;
                }
            }
        }

        private bool TryAddToolRepoAssociations(ToolRepoAssociation association)
        {
            if (association == null ||
                association.Tool == null ||
                association.Tool.Name == null)
                return false;

            if (ToolRepoAssociationsDict.TryAdd(FormatToolRepoAssociationName(association), association))
            {
                var toolName = association.Tool.Name = FormatToolName(association.Tool.Name);
                if (!ToolsDict.TryAdd(toolName, association.Tool))
                    association.Tool = ToolsDict[toolName];
                Repo.ToolAssociations.Add(association);
                return true;
            }
            else
            {
                // TODO: log this as the association already exists. 
                return false;
            }
        }

        protected bool TryAddEntities(Tool tool, Publication pub)
        {
            var toolPubAssociations = new List<ToolPublicationAssociation>
            {
                new ToolPublicationAssociation { Publication = pub }
            };

            return TryAddEntities(
                new ToolRepoAssociation() { Tool = tool },
                toolPubAssociations,
                new List<string>());
        }

        protected bool TryAddEntities(ToolRepoAssociation toolRepoAssociation, List<Publication> publications)
        {
            var toolPubAssociations = new List<ToolPublicationAssociation>();
            foreach (var pub in publications)
                toolPubAssociations.Add(new ToolPublicationAssociation() { Publication = pub });
            return TryAddEntities(toolRepoAssociation, toolPubAssociations);
        }

        protected bool TryAddEntities(
            ToolRepoAssociation toolRepoAssociation,
            List<ToolPublicationAssociation> toolPublicationAssociation,
            List<string> categoryIDs = null)
        {
            if (categoryIDs == null)
                categoryIDs = new List<string>();

            return TryAddEntities(
                new ToolInfo(
                    toolRepoAssociation,
                    toolPublicationAssociation,
                    SessionTempPath)
                {
                    CategoryIDs = categoryIDs
                });
        }

        protected bool TryAddEntities(ToolInfo info)
        {
            if (info == null)
                return false;

            foreach (var categoryID in info.CategoryIDs)
                if (Categories.TryGetValue(categoryID, out Category category))
                    info.ToolRepoAssociation.Tool.CategoryAssociations
                        .Add(new ToolCategoryAssociation()
                        {
                            Category = category
                        });

            AddToolPubAssociations(info.ToolRepoAssociation.Tool, info.ToolPubAssociations);
            return TryAddToolRepoAssociations(info.ToolRepoAssociation);
        }

        protected void UpdateAssociation(Tool tool, DateTime dateAddedToRepository)
        {
            if (tool != null &&
                ToolRepoAssociationsDict.TryGetValue(
                FormatToolRepoAssociationName(tool),
                out ToolRepoAssociation association))
                association.DateAddedToRepository = dateAddedToRepository;
        }

        protected bool TryParseBibitem(string bibitem, out Publication publication)
        {
            if (_bibitemParser.TryParse(bibitem, out publication) &&
                publication.Year != null)
            {
                if (publication.Authors != null &&
                    publication.Authors.Count > 0)
                {
                    publication.AuthorPublications = new List<AuthorPublication>();
                    foreach (var author in publication.Authors)
                        publication.AuthorPublications
                            .Add(new AuthorPublication(author, publication));
                }
                return true;
            }
            else
                return false;
        }

        public void UpdateCategories(List<Category> categories)
        {
            if (categories != null)
                foreach (var category in categories)
                    Categories.TryAdd(category.ToolShedID, category);
        }
    }
}
