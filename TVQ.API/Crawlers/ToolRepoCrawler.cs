using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Model;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Crawlers
{
    public abstract class ToolRepoCrawler
    {
        protected ConcurrentBag<Tool> _tools;
        public ReadOnlyCollection<Tool> Tools
        {
            get
            {
                return new ReadOnlyCollection<Tool>(_tools.ToArray());
            }
        }

        protected ConcurrentBag<Publication> _publications;
        public ReadOnlyCollection<Publication> Publications
        {
            get
            {
                return new ReadOnlyCollection<Publication>(_publications.ToArray());
            }
        }

        protected string _sessionTempPath;
        protected WebClient _webClient;
        protected HttpClient _httpClient;
        protected Repository _repo;

        protected readonly TVQContext _dbContext;

        public ToolRepoCrawler(TVQContext dbContext, Repository repo)
        {
            _repo = repo;
            _webClient = new WebClient();
            _httpClient = new HttpClient();
            _tools = new ConcurrentBag<Tool>();
            _publications = new ConcurrentBag<Publication>();
            _dbContext = dbContext;

            _sessionTempPath = Path.GetFullPath(Path.GetTempPath()) +
                Utilities.GetRandomString() +
                Path.DirectorySeparatorChar;

            if (Directory.Exists(_sessionTempPath))
                Directory.Delete(_sessionTempPath, true);
            Directory.CreateDirectory(_sessionTempPath);
        }

        public abstract Task ScanAsync();

        /// <summary>
        /// Gets a list of tools available in the given repository.
        /// </summary>
        /// <param name="repo">The repository to search 
        /// for a list of tools.</param>
        /// <returns>A list of the tools available in 
        /// the given repository.</returns>
        //public abstract Task<List<Tool>> GetToolsAsync(Repository repo);

        /// <summary>
        /// Gets a list of publications for the given tool 
        /// in the given repository. 
        /// </summary>
        /// <param name="repo">The repository where the tool is 
        /// located.</param>
        /// <param name="tool">The tool whose publications should be 
        /// determined.</param>
        /// <returns>A list of the publications available in the 
        /// given repo for the given tool.</returns>
        //public abstract Task<List<Publication>> GetPublicationsAsync(Repository repo, Tool tool);

        /// <summary>
        /// Get a list of publications for given tools from the 
        /// given repository. 
        /// This method efficiently implements <see cref="GetPublicationsAsync"/>
        /// for multiple tools (using the task parallel library). 
        /// </summary>
        /// <param name="repo">The repository where the tool is
        /// located.</param>
        /// <param name="tools">A list of tools whose publications
        /// should be determined.</param>
        /// <returns>A list of the publications available in the 
        /// given repo for the given list of tools.</returns>
        //public abstract Task<List<Publication>> GetPublicationsAsync(Repository repo, List<Tool> tools);
    }
}
