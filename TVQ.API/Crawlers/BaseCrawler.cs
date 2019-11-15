using Genometric.TVQ.API.Model;
using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace Genometric.TVQ.API.Crawlers
{
    public abstract class BaseCrawler : IDisposable
    {
        protected WebClient WebClient { get; }

        protected string SessionTempPath { get; }

        protected BaseCrawler()
        {
            WebClient = new WebClient();

            do
            {
                SessionTempPath =
                    Path.GetFullPath(Path.GetTempPath()) +
                    Utilities.GetRandomString(10) +
                    Path.DirectorySeparatorChar;
            }
            while (Directory.Exists(SessionTempPath));
            Directory.CreateDirectory(SessionTempPath);
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
