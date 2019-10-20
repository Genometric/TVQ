using Genometric.TVQ.API.Model;
using Newtonsoft.Json;
using System;
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
    internal class ToolShed : ToolRepoCrawler
    {
        private List<Tool> _tools;

        public ToolShed(Repository repo, List<Tool> tools) : base(repo, tools)
        { }

        public override async Task ScanAsync()
        {
            await GetToolsAsync().ConfigureAwait(false);
            await GetPublicationsAsync().ConfigureAwait(false);
        }

        private async Task GetToolsAsync()
        {
            HttpResponseMessage response = await HttpClient.GetAsync(Repo.URI);
            string content;
            if (response.IsSuccessStatusCode)
                content = await response.Content.ReadAsStringAsync();
            else
                /// TODO: replace with an exception.
                return;

            var tools = new List<Tool>(JsonConvert.DeserializeObject<List<Tool>>(content));
            _tools = new List<Tool>();

            foreach (var tool in tools)
                if (!TryAddTool(tool))
                {
                    _tools.Add(tool);
                    Repo.Tools.Add(tool);
                }
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

            foreach (var tool in _tools)
                downloader.Post(tool);
            downloader.Complete();

            await extractPublications.Completion;

            Directory.Delete(SessionTempPath, true);
        }

        private Tool Downloader(Tool tool)
        {
            WebClient.DownloadFileTaskAsync(
                address: new Uri(string.Format(
                    "https://toolshed.g2.bx.psu.edu/repos/{0}/{1}/archive/tip.zip",
                    tool.Owner,
                    tool.Name)),
                fileName: SessionTempPath + tool.ID);
            return tool;
        }

        private Tuple<Tool, string[]> WrapperExtractor(Tool tool)
        {
            /// To avoid `path traversal attacks` from malicious software, 
            /// there must be a trailing path separator at the end of the path. 
            string extractPath =
                SessionTempPath + tool.ID + "_" + new Random().Next(100000, 10000000) + "_" +
                Path.DirectorySeparatorChar;
            Directory.CreateDirectory(extractPath);

            var xmlFiles = new List<string>();
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(SessionTempPath + tool.ID))
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
                                    pub.BibTeXEntry = item.Value;
                                    break;
                            }
                            pubs.Add(pub);
                        }
                    }
                    tool.Publications.AddRange(pubs);
                }
                catch (System.Xml.XmlException e)
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
