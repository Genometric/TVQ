using Genometric.BibitemParser;
using Genometric.TVQ.API.Crawlers.ToolRepos.HelperTypes;
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
        protected Parser<ParsedPublication, Author, Keyword> BibitemParser { get; }

        protected ConcurrentDictionary<string, Tool> ToolsDict { get; }

        protected ConcurrentDictionary<string, ToolRepoAssociation> ToolRepoAssociationsDict { get; }

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

        protected BaseToolRepoCrawler(Repository repo,
                                      List<Tool> tools,
                                      List<Publication> publications,
                                      List<Category> categories) :
            base(publications)
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

            Categories = new Dictionary<string, Category>();
            UpdateCategories(categories);

            ToolDownloadRecords = new ConcurrentBag<ToolDownloadRecord>();

            BibitemParser = new Parser<ParsedPublication, Author, Keyword>(
                new ParsedPublicationConstructor(),
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

        public abstract Task ScanAsync();

        private void AddToolPubAssociations(Tool tool, List<ToolPublicationAssociation> associations)
        {
            if (associations == null || associations.Count == 0)
                return;

            foreach (var association in associations)
            {
                // Does a publication (according to the publication hash key) as the 
                // given one has already been defined?
                // If no, then keep the parsed publication and add it to the dictionary.
                if (!TryAddPublication(association.Publication))
                {
                    // Yes, then replaced the parsed publication with the one that already exists.
                    /// Note:
                    /// -----
                    /// Alternative to this is to merge the information in two citations. 
                    /// However, since the one already in PublicationsDict may have more complete
                    /// information because it may have been updated by the info from Scopus, this
                    /// approach is used. Also, merging two publications comes with many corner 
                    /// cases to be addressed.
                    association.Publication = GetPublication(association.Publication);
                }

                if (TryAddToolPublicationAssociation(tool, association))
                {
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
        }

        private bool TryAddToolRepoAssociations(ToolRepoAssociation association)
        {
            // Check if the association and the tool 
            // contains the required information.
            if (association == null ||
                association.Tool == null ||
                association.Tool.Name == null ||
                association.DateAddedToRepository == null)
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

        protected bool TryAddEntities(Tool tool, DateTime? dateAddedToRepo, Publication pub)
        {
            var toolPubAssociations = new List<ToolPublicationAssociation>
            {
                new ToolPublicationAssociation { Publication = pub }
            };

            return TryAddEntities(
                new ToolRepoAssociation() { Tool = tool, DateAddedToRepository = dateAddedToRepo },
                toolPubAssociations,
                new List<string>());
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
            if (BibitemParser.TryParse(bibitem, out ParsedPublication parsedPublication) &&
                (parsedPublication.DOI != null ||
                parsedPublication.PubMedID != null ||
                parsedPublication.Title != null))
            {
                publication = new Publication(parsedPublication);
                return true;
            }
            else
            {
                publication = null;
                return false;
            }
        }

        public void UpdateCategories(List<Category> categories)
        {
            if (categories != null)
                foreach (var category in categories)
                    Categories.TryAdd(category.ToolShedID, category);
        }
    }
}
