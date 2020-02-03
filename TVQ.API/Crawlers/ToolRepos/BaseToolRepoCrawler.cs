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

        protected BaseToolRepoCrawler(Repository repo, List<Tool> tools, List<Category> categories)
        {
            Repo = repo;
            if (tools != null)
                ToolsDict = new ConcurrentDictionary<string, Tool>(
                            tools.ToDictionary(
                                x => FormatToolName(x.Name), x => x));

            if (Repo != null)
                ToolRepoAssociationsDict =
                    new ConcurrentDictionary<string, ToolRepoAssociation>(
                        repo.ToolAssociations.ToDictionary(
                            x => FormatToolRepoAssociationName(x), x => x));

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

        public abstract Task ScanAsync();

        protected bool TryAddToolRepoAssociations(ToolRepoAssociation association)
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
            return TryAddEntities(new ToolRepoAssociation() { Tool = tool }, new List<Publication> { pub }, new List<string>());
        }

        protected bool TryAddEntities(ToolRepoAssociation association, List<Publication> pubs, List<string> categoryIDs = null)
        {
            if (categoryIDs == null)
                categoryIDs = new List<string>();

            return TryAddEntities(
                new ToolInfo(association, SessionTempPath)
                {
                    Publications = pubs,
                    CategoryIDs = categoryIDs
                });
        }

        protected bool TryAddEntities(ToolInfo info)
        {
            if (info == null)
                return false;

            if (info.Publications != null)
                foreach (var pub in info.Publications)
                {
                    pub.Tool = info.ToolRepoAssociation.Tool;
                    info.ToolRepoAssociation.Tool.Publications.Add(pub);
                }

            foreach (var categoryID in info.CategoryIDs)
                if (Categories.TryGetValue(categoryID, out Category category))
                    info.ToolRepoAssociation.Tool.CategoryAssociations
                        .Add(new ToolCategoryAssociation()
                        {
                            Category = category
                        });

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
