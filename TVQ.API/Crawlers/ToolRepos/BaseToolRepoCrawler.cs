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

            ToolDownloadRecords = new ConcurrentBag<ToolDownloadRecord>();

            _bibitemParser = new Parser<Publication, Author, Keyword>(
                new PublicationConstructor(),
                new AuthorConstructor(),
                new KeywordConstructor());
        }

        private string FormatToolName(string name)
        {
            return name.Trim().ToUpperInvariant();
        }

        public abstract Task ScanAsync();

        protected bool TryAddTool(Tool tool)
        {
            if (tool == null)
                return false;

            if (ToolsDict.TryAdd(FormatToolName(tool.Name), tool))
            {
                tool.Name = tool.Name.Trim();
                Repo.Tools.Add(tool);
                return true;
            }
            else
            {
                // TODO: handle failure of the following attempt.
                return false;
            }
        }

        protected bool TryAddEntities(Tool tool, Publication pub)
        {
            return TryAddEntities(tool, new List<Publication> { pub });
        }

        protected bool TryAddEntities(Tool tool, List<Publication> pubs)
        {
            if (tool == null)
                return false;

            if (pubs != null)
                foreach (var pub in pubs)
                {
                    pub.Tool = tool;
                    tool.Publications.Add(pub);
                }

            // TODO: handle the failure of the following.
            return TryAddTool(tool);
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
