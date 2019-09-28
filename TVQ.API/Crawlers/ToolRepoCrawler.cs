using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Crawlers
{
    public abstract class ToolRepoCrawler : IDisposable
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

        protected void AddEntities(Tool tool, List<Publication> pubs)
        {
            tool.Repository = _repo;
            tool.Publications = pubs;
            _dbContext.Tools.Add(tool);
            _dbContext.Publications.AddRange(pubs);
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
