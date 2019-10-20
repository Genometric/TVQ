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
        protected ConcurrentDictionary<string, Tool> ToolsDict { get; }
        public ReadOnlyCollection<Tool> Tools
        {
            get
            {
                return new ReadOnlyCollection<Tool>(ToolsDict.Values.ToList());
            }
        }

        private ConcurrentBag<Publication> _publications;
        public ReadOnlyCollection<Publication> Publications
        {
            get
            {
                return new ReadOnlyCollection<Publication>(_publications.ToList());
            }
        }

        public ConcurrentBag<ToolDownloadRecord> ToolDownloadRecords { get; }

        protected string SessionTempPath { get; }
        protected WebClient WebClient { get; }
        protected HttpClient HttpClient { get; }
        protected Repository Repo { get; }

        public ToolRepoCrawler(Repository repo, List<Tool> tools)
        {
            Repo = repo;
            WebClient = new WebClient();
            HttpClient = new HttpClient();

            ToolsDict = new ConcurrentDictionary<string, Tool>(
                        tools.ToDictionary(
                            x => x.Name, x => x));

            _publications = new ConcurrentBag<Publication>();
            ToolDownloadRecords = new ConcurrentBag<ToolDownloadRecord>();

            SessionTempPath = Path.GetFullPath(Path.GetTempPath()) +
                Utilities.GetRandomString() +
                Path.DirectorySeparatorChar;

            if (Directory.Exists(SessionTempPath))
                Directory.Delete(SessionTempPath, true);
            Directory.CreateDirectory(SessionTempPath);
        }

        public abstract Task ScanAsync();

        protected bool TryAddTool(Tool tool)
        {
            tool.Name = tool.Name.Trim();
            if (ToolsDict.ContainsKey(tool.Name))
            {
                // TODO: log this
                return false;
            }

            // TODO: handle failure of the following attempt. 
            ToolsDict.TryAdd(tool.Name, tool);
            Repo.Tools.Add(tool);
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
            if (TryAddTool(tool))
                foreach (var pub in pubs)
                    _publications.Add(pub);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Directory.Delete(SessionTempPath, true);
        }
    }
}
