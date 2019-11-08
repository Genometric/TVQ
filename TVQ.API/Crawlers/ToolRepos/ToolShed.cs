using Genometric.BibitemParser;
using Genometric.TVQ.API.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;

namespace Genometric.TVQ.API.Crawlers.ToolRepos
{
    internal class ToolShed : BaseToolRepoCrawler
    {
        private int _maxParallelDownloads = 3;
        private int _maxParallelActions = Environment.ProcessorCount * 3;
        private int _boundedCapacity = Environment.ProcessorCount * 3;

        public ToolShed(Repository repo) : base(repo)
        { }

        public override async Task ScanAsync()
        {
            var tools = await GetToolsAsync().ConfigureAwait(false);
            if (tools != null)
                await GetPublicationsAsync(tools).ConfigureAwait(false);
        }

        private async Task<List<Tool>> GetToolsAsync()
        {
            HttpResponseMessage response = await HttpClient.GetAsync(Repo.URI).ConfigureAwait(false);
            string content;
            if (response.IsSuccessStatusCode)
                content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            else
                /// TODO: replace with an exception.
                return null;

             return new List<Tool>(JsonConvert.DeserializeObject<List<Tool>>(content));
        }

        private async Task GetPublicationsAsync(List<Tool> tools)
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
                    MaxDegreeOfParallelism = _maxParallelDownloads
                });

            var extractXMLs = new TransformBlock<Tool, Tuple<Tool, string[]>>(
                new Func<Tool, Tuple<Tool, string[]>>(WrapperExtractor),
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = _boundedCapacity,
                    MaxDegreeOfParallelism = _maxParallelActions
                });

            var extractPublications = new ActionBlock<Tuple<Tool, string[]>>(
                input => { ExtractPublications(input); },
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = _boundedCapacity,
                    MaxDegreeOfParallelism = _maxParallelActions
                });

            downloader.LinkTo(extractXMLs, linkOptions);
            extractXMLs.LinkTo(extractPublications, linkOptions);

            foreach (var tool in tools)
                downloader.Post(tool);
            downloader.Complete();

            await extractPublications.Completion;

            Directory.Delete(SessionTempPath, true);
        }

        private Tool Downloader(Tool tool)
        {
            /// Note: do not use base WebClient, because WebClient cannot 
            /// download multiple files concurrently.
            new WebClient().DownloadFileTaskAsync(
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
                            switch (item.Attribute("type").Value.Trim().ToLower())
                            {
                                case "doi":
                                    pubs.Add(new Publication() { DOI = item.Value });
                                    break;

                                case "bibtex":
                                    var parser = new Parser<Publication, Author, Keyword>(new PublicationConstructor(), new AuthorConstructor(), new KeywordConstructor());
                                    if (parser.TryParse(item.Value, out Publication pub))
                                        pubs.Add(pub);
                                    break;
                            }
                        }
                    }

                    TryAddEntities(tool, pubs);
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
