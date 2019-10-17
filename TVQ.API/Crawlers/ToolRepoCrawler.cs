using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Crawlers
{
    public abstract class ToolRepoCrawler : IDisposable
    {
        public ConcurrentDictionary<string, Tool> Tools { get; }

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

        public ToolRepoCrawler(
            TVQContext dbContext, 
            Repository repo)
        {
            _repo = repo;
            _webClient = new WebClient();
            _httpClient = new HttpClient();
            _publications = new ConcurrentBag<Publication>();
            _dbContext = dbContext;
            Tools =
                new ConcurrentDictionary<string, Tool>(
                    _dbContext.Tools.ToDictionary(
                        x => x.Name, x => x));

            _sessionTempPath = Path.GetFullPath(Path.GetTempPath()) +
                Utilities.GetRandomString() +
                Path.DirectorySeparatorChar;

            if (Directory.Exists(_sessionTempPath))
                Directory.Delete(_sessionTempPath, true);
            Directory.CreateDirectory(_sessionTempPath);
        }

        public abstract Task ScanAsync();

        protected async Task<bool> TryAddTool(Tool tool)
        {
            tool.Name = tool.Name.Trim();
            if (Tools.ContainsKey(tool.Name))
            {
                // TODO: log this
                return false;
            }

            tool.Repository = _repo;

            // TODO: handle failure of the following attempt. 
            Tools.TryAdd(tool.Name, tool);
            await _dbContext.Tools.AddAsync(tool);
            return true;
        }

        protected async Task TryAddEntities(Tool tool, Publication pub)
        {
            await TryAddEntities(tool, new List<Publication> { pub }).ConfigureAwait(false);
        }

        protected async Task TryAddEntities(Tool tool, List<Publication> pubs)
        {
            foreach (var pub in pubs)
                pub.Tool = tool;
            tool.Publications = pubs;

            // TODO: handle the failure of the following.
            await TryAddTool(tool).ConfigureAwait(false);
            await _dbContext.Publications.AddRangeAsync(pubs).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Directory.Delete(_sessionTempPath, true);
        }
    }
}
