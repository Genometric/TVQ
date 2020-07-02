using Genometric.BibitemParser;
using Genometric.TVQ.API.Crawlers.ToolRepos.HelperTypes;
using Genometric.TVQ.API.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.API.Model;
using Genometric.TVQ.API.Model.Associations;
using Genometric.TVQ.API.Model.JsonConverters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Crawlers.ToolRepos
{
    public abstract class BaseToolRepoCrawler : BaseCrawler
    {
        protected Parser<ParsedPublication, Author, Keyword> BibitemParser { get; }

        protected ConcurrentDictionary<string, Tool> Tools { get; }

        protected ConcurrentDictionary<string, ToolRepoAssociation> ToolRepoAssociationsDict { get; }

        protected JsonSerializerSettings ToolJsonSerializerSettings { set; get; }
        protected JsonSerializerSettings ToolRepoAssoJsonSerializerSettings { set; get; }
        protected JsonSerializerSettings PublicationSerializerSettings { set; get; }
        protected JsonSerializerSettings CategorySerializerSettings { set; get; }

        private readonly Dictionary<string, Category> _categories;
        private readonly Dictionary<string, CategoryRepoAssociation> _categoryRepoAssociations;

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
                Tools = new ConcurrentDictionary<string, Tool>(
                            tools.ToDictionary(
                                x => x.Name,
                                x => x),
                            StringComparer.InvariantCultureIgnoreCase);

            if (Repo != null)
                ToolRepoAssociationsDict =
                    new ConcurrentDictionary<string, ToolRepoAssociation>(
                        repo.ToolAssociations.ToDictionary(
                            x => FormatToolRepoAssociationName(x.Tool),
                            x => x));

            _categories = new Dictionary<string, Category>();
            _categoryRepoAssociations = new Dictionary<string, CategoryRepoAssociation>();
            foreach (var category in categories)
                foreach (var association in category.RepoAssociations)
                    EnsureEntity(association);

            ToolDownloadRecords = new ConcurrentBag<ToolDownloadRecord>();

            BibitemParser = new Parser<ParsedPublication, Author, Keyword>(
                new ParsedPublicationConstructor(),
                new AuthorConstructor(),
                new KeywordConstructor());
        }

        private List<string> GetKeys(CategoryRepoAssociation association)
        {
            var repo = association.Repository == null ? Repo.Name : association.Repository.Name;
            var id = association.IDinRepo ?? string.Empty;
            var name = association.Category == null ? string.Empty : association.Category.Name ?? string.Empty;

            if (string.IsNullOrWhiteSpace(id) && string.IsNullOrWhiteSpace(name))
                return new List<string>();

            string a = null, b = null, c = null;

            a = string.Join("::", repo, id, name);

            if (!string.IsNullOrWhiteSpace(name))
                b = string.Join("::", repo, string.Empty, name);

            if (!string.IsNullOrWhiteSpace(id))
                c = string.Join("::", repo, id, string.Empty);

            var keys = new List<string> { a };
            if (b != null && b != a)
                keys.Add(b);

            if (c != null && c != b && c != a)
                keys.Add(c);

            return keys;
        }

        protected CategoryRepoAssociation EnsureEntity(CategoryRepoAssociation association)
        {
            if (association == null ||
                (association.IDinRepo == null && association.Category == null))
                return null;

            association.Category = EnsureEntity(association.Category);
            if (association.Category == null)
                association.Category = new Category();

            var keys = GetKeys(association);
            var rtv = association;
            foreach (var key in keys)
                if (!_categoryRepoAssociations.TryGetValue(key, out rtv))
                {
                    _categoryRepoAssociations.Add(key, association);

                    // This assignment is necessary because if `key`
                    // does not exist in `_categoryRepoAssociations` then 
                    // rtv will be set to null. 
                    rtv = association;
                }

            return rtv;
        }

        protected Category EnsureEntity(Category category)
        {
            if (category == null) return null;
            if (category.Name == null)
                return category;

            var name = category.Name.ToUpperInvariant();
            if (!_categories.TryGetValue(name, out Category rtv))
            {
                _categories.Add(name, category);
                rtv = category;
            }
            return rtv;
        }

        private string FormatToolRepoAssociationName(Tool tool)
        {
            return Repo.Name + "::" + tool.Name.Trim();
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

            var toolName = info.ToolRepoAssociation.Tool.Name = info.ToolRepoAssociation.Tool.Name.Trim();
            if (!Tools.TryAdd(toolName, info.ToolRepoAssociation.Tool))
                info.ToolRepoAssociation.Tool = Tools[toolName];

            // TODO: there could be a better way of associating categories with 
            // repository if the tool association was successful than this method.
            var categoryRepoAssoToRegister = new List<CategoryRepoAssociation>();

            foreach (var association in info.CategoryRepoAssociations)
            {
                var asso = EnsureEntity(association);
                info.ToolRepoAssociation.Tool.CategoryAssociations
                    .Add(new ToolCategoryAssociation()
                    {
                        Category = asso.Category,
                        Tool = info.ToolRepoAssociation.Tool
                    });

                categoryRepoAssoToRegister.Add(asso);
            }

            AddToolPubAssociations(info.ToolRepoAssociation.Tool, info.ToolPubAssociations);
            if (TryAddToolRepoAssociations(info))
            {
                foreach (var association in categoryRepoAssoToRegister)
                    Repo.CategoryAssociations.Add(association);
                return true;
            }
            else
                return false;
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
