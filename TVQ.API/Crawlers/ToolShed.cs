using Genometric.TVQ.API.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;

namespace Genometric.TVQ.API.Crawlers
{
    internal class ToolShed
    {
        private readonly HttpClient _client;
        private string _sessionTempPath;
        private ConcurrentBag<Publication> _publications;

        public ToolShed()
        {
            _client = new HttpClient();
            _publications = new ConcurrentBag<Publication>();

            _sessionTempPath = Path.GetFullPath(Path.GetTempPath()) +
                new Random().Next(100000, 10000000) +
                Path.DirectorySeparatorChar;
            if (Directory.Exists(_sessionTempPath))
                Directory.Delete(_sessionTempPath, true);
            Directory.CreateDirectory(_sessionTempPath);
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
            var execOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 50
            };

            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            var downloader = new TransformBlock<Tool, Tool>(new Func<Tool, Tool>(Downloader), execOptions);
            var extracXMLs = new TransformBlock<Tool, Tuple<Tool, string[]>>(new Func<Tool, Tuple<Tool, string[]>>(WrapperExtractor), execOptions);
            var extractPublications = new ActionBlock<Tuple<Tool, string[]>>(input => { ExtractPublications(input); }, execOptions);

            downloader.LinkTo(extracXMLs, linkOptions);
            extracXMLs.LinkTo(extractPublications, linkOptions);

            foreach (var tool in tools)
                downloader.Post(tool);

            downloader.Complete();

            await extractPublications.Completion;

            Directory.Delete(_sessionTempPath, true);
            return _publications.ToList();
        }

        private Tool Downloader(Tool tool)
        {
            new System.Net.WebClient().DownloadFileTaskAsync(
                address: new Uri(string.Format(
                    "https://toolshed.g2.bx.psu.edu/repos/{0}/{1}/archive/tip.zip",
                    tool.Owner,
                    tool.Name)),
                fileName: _sessionTempPath + tool.Id);
            return tool;
        }

        private Tuple<Tool, string[]> WrapperExtractor(Tool tool)
        {
            /// To avoid `path traversal attacks` from malicious software, 
            /// there must be a trailing path separator at the end of the path. 
            string extractPath =
                _sessionTempPath + tool.Id + "_" + new Random().Next(100000, 10000000) + "_" +
                Path.DirectorySeparatorChar;
            Directory.CreateDirectory(extractPath);

            var xmlFiles = new List<string>();
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(_sessionTempPath + tool.Id))
                    foreach (ZipArchiveEntry entry in archive.Entries)
                        if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        {
                            var extractedFileName = extractPath + Path.GetFileName(entry.FullName);
                            entry.ExtractToFile(extractedFileName);
                            xmlFiles.Add(extractedFileName);
                        }
            }
            catch (InvalidDataException e)
            {
                /// This exception is thrown when the Zip archive
                /// cannot be read.
            }

            return new Tuple<Tool, string[]>(tool, xmlFiles.ToArray<string>());
        }

        private void ExtractPublications(Tuple<Tool, string[]> input)
        {
            var tool = input.Item1;
            var xmlFiles = input.Item2;
            foreach (var filename in xmlFiles)
            {
                try
                {
                    XElement toolDoc = XElement.Load(filename);

                    foreach (var item in toolDoc.Elements("citations").Descendants())
                    {
                        if (item.Attribute("type") != null)
                        {
                            switch (item.Attribute("type").Value.Trim().ToLower())
                            {
                                case "doi":
                                    _publications.Add(new Publication()
                                    {
                                        ToolId = tool.Id,
                                        DOI = item.Value
                                    });
                                    break;

                                case "bibtex":
                                    _publications.Add(new Publication()
                                    {
                                        ToolId = tool.Id,
                                        Citation = item.Value
                                    });
                                    break;
                            }
                        }
                    }
                }
                catch(System.Xml.XmlException e)
                {
                    /// This exception may happen if the XML 
                    /// file has multiple roots.
                }
            }
        }
    }
}
