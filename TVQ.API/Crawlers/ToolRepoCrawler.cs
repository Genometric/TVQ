using Genometric.TVQ.API.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Crawlers
{
    public abstract class ToolRepoCrawler : IDisposable
    {
        public ConcurrentDictionary<string, Tool> Tools { internal set; get; }

        public ConcurrentBag<Publication> Publications { get; }

        public ConcurrentBag<ToolDownloadRecord> ToolDownloadRecords { get; }

        protected string _sessionTempPath;
        protected WebClient _webClient;
        protected HttpClient _httpClient;
        protected Repository _repo;

        public ToolRepoCrawler(Repository repo)
        {
            _repo = repo;
            _webClient = new WebClient();
            _httpClient = new HttpClient();

            Tools = new ConcurrentDictionary<string, Tool>();
            Publications = new ConcurrentBag<Publication>();
            ToolDownloadRecords = new ConcurrentBag<ToolDownloadRecord>();

            _sessionTempPath = Path.GetFullPath(Path.GetTempPath()) +
                Utilities.GetRandomString() +
                Path.DirectorySeparatorChar;

            if (Directory.Exists(_sessionTempPath))
                Directory.Delete(_sessionTempPath, true);
            Directory.CreateDirectory(_sessionTempPath);
        }

        public abstract Task ScanAsync();

        protected bool TryAddTool(Tool tool)
        {
            tool.Name = tool.Name.Trim();
            if (Tools.ContainsKey(tool.Name))
            {
                // TODO: log this
                return false;
            }

            // TODO: handle failure of the following attempt. 
            Tools.TryAdd(tool.Name, tool);
            _repo.Tools.Add(tool);
            return true;
        }

        protected void TryAddEntities(Tool tool, Publication pub)
        {
            TryAddEntities(tool, new List<Publication> { pub });
        }

        protected void TryAddEntities(Tool tool, List<Publication> pubs)
        {
            foreach (var pub in pubs)
                pub.Tool = tool;
            tool.Publications.AddRange(pubs);

            // TODO: handle the failure of the following.
            TryAddTool(tool);
            foreach (var pub in pubs)
                Publications.Add(pub);
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
