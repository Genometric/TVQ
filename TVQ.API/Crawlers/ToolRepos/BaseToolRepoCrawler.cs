using Genometric.BibitemParser;
using Genometric.TVQ.API.Crawlers.ToolRepos.HelperTypes;
using Genometric.TVQ.API.Model;
using Genometric.TVQ.API.Model.Associations;
using Genometric.TVQ.API.Model.JsonConverters;
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

        private readonly Dictionary<string, Category> _categoriesByToolShedID;
        private readonly Dictionary<string, Category> _categoriesByName;

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
                            x => FormatToolRepoAssociationName(x.Tool),
                            x => x));

            Categories = new Dictionary<string, Category>();
            UpdateCategories(categories);

            _categoriesByName = new Dictionary<string, Category>();
            _categoriesByToolShedID = new Dictionary<string, Category>();
            if (categories != null)
                foreach (var category in categories)
                    EnsureCategory(category);

            ToolDownloadRecords = new ConcurrentBag<ToolDownloadRecord>();

            BibitemParser = new Parser<ParsedPublication, Author, Keyword>(
                new ParsedPublicationConstructor(),
                new AuthorConstructor(),
                new KeywordConstructor());
        }

        protected Category EnsureCategory(Category category)
        {
            Category rtv = null;
            if (category == null)
                return rtv;

            if (category.Name != null)
            {
                var name = category.Name.ToUpperInvariant();
                if (!_categoriesByName.TryGetValue(name, out rtv))
                {
                    rtv = category;
                    _categoriesByName.Add(name, category);
                }
            }

            if (category.ToolShedID != null)
            {
                if (!_categoriesByToolShedID.TryGetValue(category.ToolShedID, out rtv))
                {
                    rtv = category;
                    _categoriesByToolShedID.Add(category.ToolShedID, category);
                }
            }

            return rtv;
        }

        protected static string FormatToolName(string name)
        {
            return name.Trim().ToUpperInvariant();
        }

        private string FormatToolRepoAssociationName(Tool tool)
        {
            return Repo.Name + "::" + FormatToolName(tool.Name); ;
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
                    association.Tool.PublicationAssociations.Add(association);

                    // todo: this may not be needed.
                    association.Publication.ToolAssociations.Add(association);
                }
                else
                {
                    // This association already exists.
                    // TODO: log this.
                    continue;
                }
            }
        }

        private bool TryAddToolRepoAssociations(DeserializedInfo info)
        {
            if (ToolRepoAssociationsDict.TryAdd(FormatToolRepoAssociationName(info.ToolRepoAssociation.Tool), info.ToolRepoAssociation))
            {
                Repo.ToolAssociations.Add(info.ToolRepoAssociation);
                return true;
            }
            else
            {
                // TODO: log this as the association already exists. 
                return false;
            }
        }

        protected bool TryAddEntities(DeserializedInfo info)
        {
            // Checks if the association and the tool 
            // contains the required information.
            if (info == null ||
                info.ToolRepoAssociation.Tool == null ||
                info.ToolRepoAssociation.Tool.Name == null ||
                info.ToolRepoAssociation.DateAddedToRepository == null)
                return false;

            var toolName = info.ToolRepoAssociation.Tool.Name = FormatToolName(info.ToolRepoAssociation.Tool.Name);
            if (!ToolsDict.TryAdd(toolName, info.ToolRepoAssociation.Tool))
                info.ToolRepoAssociation.Tool = ToolsDict[toolName];

            foreach (var parsedCategory in info.Categories)
            {
                var category = EnsureCategory(parsedCategory);
                if (category == null)
                    category = parsedCategory;
                info.ToolRepoAssociation.Tool.CategoryAssociations
                    .Add(new ToolCategoryAssociation()
                    {
                        Category = category,
                        Tool = info.ToolRepoAssociation.Tool
                    });
            }

            AddToolPubAssociations(info.ToolRepoAssociation.Tool, info.ToolPubAssociations);
            return TryAddToolRepoAssociations(info);
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
                    if (category.ToolShedID != null)
                        Categories.TryAdd(category.ToolShedID, category);
        }
    }
}
