using Genometric.TVQ.API.Infrastructure;
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
    internal class ToolShed: ToolRepoCrawler
    {
        public ToolShed(TVQContext dbContext, Repository repo) :
            base(dbContext, repo)
        { }

        public override async Task ScanAsync()
        {
            await GetToolsAsync();
            _dbContext.Tools.AddRange(Tools);
            await GetPublicationsAsync();
            _dbContext.Publications.AddRange(Publications);
        }

        private async Task GetToolsAsync()
        {
            HttpResponseMessage response = await _httpClient.GetAsync(_repo.URI);
            string content;
            if (response.IsSuccessStatusCode)
                content = await response.Content.ReadAsStringAsync();
            else
                /// TODO: replace with an exception.
                return;

            _tools = new ConcurrentBag<Tool>(JsonConvert.DeserializeObject<List<Tool>>(content));
            foreach (var tool in _tools)
                tool.Repository = _repo;
        }

        private async Task GetPublicationsAsync()
        {
            // This method is implemented using the Task Parallel Library (TPL).
            // Read the following page for more info on the flow: 
            // https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl

            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            var downloader = new TransformBlock<Tool, Tool>(
                new Func<Tool, Tool>(Downloader),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 3
                });

            var extractXMLs = new TransformBlock<Tool, Tuple<Tool, string[]>>(
                new Func<Tool, Tuple<Tool, string[]>>(WrapperExtractor),
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = Environment.ProcessorCount * 3,
                    MaxDegreeOfParallelism = Environment.ProcessorCount * 3
                });

            var extractPublications = new ActionBlock<Tuple<Tool, string[]>>(
                input => { ExtractPublications(input); },
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = Environment.ProcessorCount * 3,
                    MaxDegreeOfParallelism = Environment.ProcessorCount * 3
                });

            downloader.LinkTo(extractXMLs, linkOptions);
            extractXMLs.LinkTo(extractPublications, linkOptions);

            int c = 10;
            foreach (var tool in _tools)
            {
                downloader.Post(tool);
                if (c-- == 0)
                    break;
            }
            downloader.Complete();

            await extractPublications.Completion;

            Directory.Delete(_sessionTempPath, true);
        }

        private Tool Downloader(Tool tool)
        {
            _webClient.DownloadFileTaskAsync(
                address: new Uri(string.Format(
                    "https://toolshed.g2.bx.psu.edu/repos/{0}/{1}/archive/tip.zip",
                    tool.Owner,
                    tool.Name)),
                fileName: _sessionTempPath + tool.ID);
            return tool;
        }

        private Tuple<Tool, string[]> WrapperExtractor(Tool tool)
        {
            /// To avoid `path traversal attacks` from malicious software, 
            /// there must be a trailing path separator at the end of the path. 
            string extractPath =
                _sessionTempPath + tool.ID + "_" + new Random().Next(100000, 10000000) + "_" +
                Path.DirectorySeparatorChar;
            Directory.CreateDirectory(extractPath);

            var xmlFiles = new List<string>();
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(_sessionTempPath + tool.ID))
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
                    var pubs = new List<Publication>();
                    foreach (var item in toolDoc.Elements("citations").Descendants())
                    {
                        if (item.Attribute("type") != null)
                        {
                            var pub = new Publication();
                            switch (item.Attribute("type").Value.Trim().ToLower())
                            {
                                case "doi":
                                    pub.DOI = item.Value;
                                    break;

                                case "bibtex":
                                    pub.TotalCitationCount = item.Value;
                                    break;
                            }
                            pubs.Add(pub);
                        }
                    }
                    tool.Publications = pubs;
                }
                catch(System.Xml.XmlException e)
                {
                    /// This exception may happen if the XML 
                    /// file has multiple roots.
                }
                finally
                {
                    File.Delete(filename);
                }
            }
        }
    }
}
