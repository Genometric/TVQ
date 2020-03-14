using Genometric.BibitemParser;
using Genometric.TVQ.API.Crawlers.ToolRepos.HelperTypes;
using Genometric.TVQ.API.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.API.Model;
using Genometric.TVQ.API.Model.Associations;
using Genometric.TVQ.API.Model.JsonConverters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        protected JsonSerializerSettings ToolJsonSerializerSettings { set; get; }
        protected JsonSerializerSettings ToolRepoAssoJsonSerializerSettings { set; get; }
        protected JsonSerializerSettings PublicationSerializerSettings { set; get; }
        protected JsonSerializerSettings CategorySerializerSettings { set; get; }

        private readonly Dictionary<string, CategoryRepoAssociation> _categoryRepoAssociationsByName;
        private readonly Dictionary<string, CategoryRepoAssociation> _categoryRepoAssociationsByIDInRepo;

        public ReadOnlyCollection<Tool> Tools
        {
            get
            {
                return new ReadOnlyCollection<Tool>(ToolsDict.Values.ToList());
            }
        }

        public ConcurrentBag<ToolDownloadRecord> ToolDownloadRecords { get; }

        protected Repository Repo { get; }

        protected ILogger<BaseService<RepoCrawlingJob>> Logger { get; }

        protected BaseToolRepoCrawler(Repository repo,
                                      List<Tool> tools,
                                      List<Publication> publications,
                                      List<Category> categories,
                                      ILogger<BaseService<RepoCrawlingJob>> logger) :
            base(publications)
        {
            Logger = logger;
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

            _categoryRepoAssociationsByName = new Dictionary<string, CategoryRepoAssociation>();
            _categoryRepoAssociationsByIDInRepo = new Dictionary<string, CategoryRepoAssociation>();
            foreach (var category in categories)
                foreach (var association in category.RepoAssociations)
                    EnsureEntity(association);

            ToolDownloadRecords = new ConcurrentBag<ToolDownloadRecord>();

            BibitemParser = new Parser<ParsedPublication, Author, Keyword>(
                new ParsedPublicationConstructor(),
                new AuthorConstructor(),
                new KeywordConstructor());
        }

        protected CategoryRepoAssociation EnsureEntity(CategoryRepoAssociation association)
        {
            if (association == null)
                return null;

            if (association.Category == null && association.IDinRepo != null)
                association.Category = new Category();
            else
                return null;

            CategoryRepoAssociation rtv = null;
            if (association.Category.Name == null && association.IDinRepo != null)
            {
                if (!_categoryRepoAssociationsByIDInRepo.TryGetValue(association.IDinRepo, out rtv))
                {
                    _categoryRepoAssociationsByIDInRepo.Add(association.IDinRepo, association);
                    rtv = association;
                }
            }
            else if (association.Category.Name != null)
            {
                var name = association.Category.Name.ToUpperInvariant();
                if (association.IDinRepo == null)
                {
                    if (!_categoryRepoAssociationsByName.TryGetValue(name, out rtv))
                    {
                        _categoryRepoAssociationsByName.Add(name, association);
                        rtv = association;
                    }
                }
                else
                {
                    if (_categoryRepoAssociationsByIDInRepo.TryGetValue(association.IDinRepo, out CategoryRepoAssociation existingAssociation))
                    {
                        if (existingAssociation.Category.Name != association.Category.Name)
                        {
                            _categoryRepoAssociationsByIDInRepo[association.IDinRepo].Category.Name = association.Category.Name;
                            _categoryRepoAssociationsByName.Remove(name);
                            _categoryRepoAssociationsByName.Add(name, association);
                        }
                    }
                    else
                    {
                        _categoryRepoAssociationsByIDInRepo.Add(association.IDinRepo, association);
                    }

                    rtv = _categoryRepoAssociationsByIDInRepo[association.IDinRepo];
                }
            }
            // The `Name == null && IDinRepo == null` is not possible by design.

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
                    Logger.LogDebug($"Association between Tool {tool.Name} and Repository {Repo.Name} already exists.");
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
                Logger.LogDebug($"Association between Tool {info.ToolRepoAssociation.Tool.Name} and Repository {Repo.Name} already exists.");
                return false;
            }
        }

        protected bool TryAddEntities(DeserializedInfo info)
        {
            // Checks if the association and the tool 
            // contains the required information.
            if (info == null ||
                info.ToolRepoAssociation.Tool == null)
                return false;

            if (info.ToolRepoAssociation.Tool.Name == null)
            {
                Logger.LogDebug("Skipping tool because missing name.");
                return false;
            }

            if (info.ToolRepoAssociation.DateAddedToRepository == null)
            {
                Logger.LogDebug($"Skipping tool {info.ToolRepoAssociation.Tool.Name} because the data it was added to repository is not set.");
                return false;
            }

            var toolName = info.ToolRepoAssociation.Tool.Name = FormatToolName(info.ToolRepoAssociation.Tool.Name);
            if (!ToolsDict.TryAdd(toolName, info.ToolRepoAssociation.Tool))
                info.ToolRepoAssociation.Tool = ToolsDict[toolName];

            foreach (var association in info.CategoryRepoAssociations)
            {
                var asso = EnsureEntity(association);
                info.ToolRepoAssociation.Tool.CategoryAssociations
                    .Add(new ToolCategoryAssociation()
                    {
                        Category = asso.Category,
                        Tool = info.ToolRepoAssociation.Tool
                    });

                if (asso.Repository == null)
                    Repo.CategoryAssociations.Add(asso);
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
    }
}
