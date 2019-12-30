using Genometric.BibitemParser;
using Genometric.TVQ.API.Model;
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

        public ReadOnlyCollection<Tool> Tools
        {
            get
            {
                return new ReadOnlyCollection<Tool>(ToolsDict.Values.ToList());
            }
        }

        public ConcurrentBag<ToolDownloadRecord> ToolDownloadRecords { get; }


        protected Repository Repo { get; }

        protected BaseToolRepoCrawler(Repository repo, List<Tool> tools)
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

            ToolDownloadRecords = new ConcurrentBag<ToolDownloadRecord>();

            _bibitemParser = new Parser<Publication, Author, Keyword>(
                new PublicationConstructor(),
                new AuthorConstructor(),
                new KeywordConstructor());
        }

        private static string FormatToolName(string name)
        {
            return name.Trim().ToUpperInvariant();
        }

        private static string FormatToolRepoAssociationName(ToolRepoAssociation association)
        {
            return association.Repository + "::" + FormatToolName(association.Tool.Name);
        }

        public abstract Task ScanAsync();

        protected bool TryAddToolRepoAssociations(ToolRepoAssociation association)
        {
            if (association == null)
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
            return TryAddEntities(new ToolRepoAssociation() { Tool = tool }, pub);
        }

        protected bool TryAddEntities(Tool tool, List<Publication> pubs)
        {
            return TryAddEntities(new ToolRepoAssociation() { Tool = tool }, pubs);
        }

        protected bool TryAddEntities(ToolRepoAssociation association, Publication pub)
        {
            return TryAddEntities(association, new List<Publication> { pub });
        }

        protected bool TryAddEntities(ToolRepoAssociation association, List<Publication> pubs)
        {
            if (association == null)
                return false;

            if (pubs != null)
                foreach (var pub in pubs)
                {
                    pub.Tool = association.Tool;
                    association.Tool.Publications.Add(pub);
                }

            // TODO: handle the failure of the following.
            return TryAddToolRepoAssociations(association);
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
    }
}
