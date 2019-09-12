using Genometric.TVQ.API.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Genometric.TVQ.API.Crawlers
{
    internal class ToolShed
    {
        private readonly HttpClient _client;

        public ToolShed()
        {
            _client = new HttpClient();
        }

        public async Task<List<Tool>> GetTools(Repository repo)
        {
            HttpResponseMessage response = await _client.GetAsync(repo.URI);
            string content;
            if (response.IsSuccessStatusCode)
                content = await response.Content.ReadAsStringAsync();
            else
                /// TODO: replace with an exception.
                return new List<Tool>();

            var tools = JsonConvert.DeserializeObject<List<Tool>>(content);
            foreach (var tool in tools)
                tool.Repo = repo;
            repo.ToolCount += tools.Count;

            return tools;
        }

        public async Task<List<Publication>> GetPublications(Repository repo, List<Tool> tools)
        {
            var pubs = new List<Publication>();
            var rnd = new Random();
            var tmpPath =
                Path.GetFullPath(Path.GetTempPath()) +
                rnd.Next(100000, 10000000) +
                Path.DirectorySeparatorChar;
            if (Directory.Exists(tmpPath))
                Directory.Delete(tmpPath, true);
            Directory.CreateDirectory(tmpPath);

            foreach (var tool in tools)
            {
                string zipFileName = tmpPath + tool.Id;
                try
                {
                    /// TODO: creating a new client for every request 
                    /// maybe way too expensive. Maybe should check if 
                    /// client can run multiple concurrent requests in 
                    /// a thread-safe fashion?
                    new System.Net.WebClient().DownloadFile(
                        address: new Uri(string.Format(
                            "https://toolshed.g2.bx.psu.edu/repos/{0}/{1}/archive/tip.zip",
                            tool.Owner,
                            tool.Name)),
                        fileName: zipFileName);
                }
                catch(Exception e)
                {

                }

                /// Normalizes the path.
                /// To avoid `path traversal attacks` from malicious software, 
                /// there must be a trailing path separator at the end of the path. 
                string extractPath =
                    tmpPath + tool.Id + "_" + rnd.Next(100000, 10000000) + "_" +
                    Path.DirectorySeparatorChar;
                Directory.CreateDirectory(extractPath);

                try
                {
                    using (ZipArchive archive = ZipFile.OpenRead(zipFileName))
                        foreach (ZipArchiveEntry entry in archive.Entries)
                            if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                            {
                                var extractedFileName = extractPath + Path.GetFileName(entry.FullName);
                                entry.ExtractToFile(extractedFileName);
                                pubs.AddRange(ExtractCitation(extractedFileName, tool));
                            }
                }
                catch (InvalidDataException e)
                {
                    /// This exception is thrown when the Zip archive
                    /// cannot be read.
                }
                catch(Exception e)
                {

                }
            }

            Directory.Delete(tmpPath, true);
            return pubs;
        }

        private List<Publication> ExtractCitation(string filename, Tool tool)
        {
            var pubs = new List<Publication>();
            XElement toolDoc = XElement.Load(filename);

            foreach (var item in toolDoc.Elements("citations").Descendants())
                switch (item.Attribute("type").Value.Trim().ToLower())
                {
                    case "doi":
                        pubs.Add(new Publication()
                        {
                            ToolId = tool.Id,
                            DOI = item.Value
                        });
                        break;

                    case "bibtex":
                        pubs.Add(new Publication()
                        {
                            ToolId = tool.Id,
                            Citation = item.Value
                        });
                        break;
                }

            return pubs;
        }
    }
}
