using Genometric.BibitemParser;
using Genometric.TVQ.API.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;

namespace Genometric.TVQ.API.Crawlers.ToolRepos
{
    internal class ToolShed : BaseToolRepoCrawler
    {
        private readonly int _maxParallelDownloads = 3;
        private readonly int _maxParallelActions = Environment.ProcessorCount * 3;
        private readonly int _boundedCapacity = Environment.ProcessorCount * 3;

        private readonly ExecutionDataflowBlockOptions _downloadExeOptions;
        private readonly ExecutionDataflowBlockOptions _xmlExtractExeOptions;
        private readonly ExecutionDataflowBlockOptions _pubExtractExeOptions;

        public ToolShed(Repository repo) : base(repo)
        {
            _downloadExeOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = _maxParallelDownloads
            };

            _xmlExtractExeOptions = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = _boundedCapacity,
                MaxDegreeOfParallelism = _maxParallelActions
            };

            _pubExtractExeOptions = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = _boundedCapacity,
                MaxDegreeOfParallelism = _maxParallelActions
            };
        }

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

        /// <summary>
        /// This method is implemented using the Task Parallel Library (TPL).
        //. https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl
        /// </summary>
        private async Task GetPublicationsAsync(List<Tool> tools)
        {
            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            var downloader = new TransformBlock<ToolInfo, ToolInfo>(
                new Func<ToolInfo, ToolInfo>(Downloader), _downloadExeOptions);

            var extractXMLs = new TransformBlock<ToolInfo, ToolInfo>(
                new Func<ToolInfo, ToolInfo>(WrapperExtractor), _xmlExtractExeOptions);

            var extractPublications = new ActionBlock<ToolInfo>(
                input => { ExtractPublications(input); }, _pubExtractExeOptions);

            downloader.LinkTo(extractXMLs, linkOptions);
            extractXMLs.LinkTo(extractPublications, linkOptions);

            foreach (var tool in tools)
                downloader.Post(new ToolInfo(tool, SessionTempPath));
            downloader.Complete();

            await extractPublications.Completion.ConfigureAwait(false);

            Directory.Delete(SessionTempPath, true);
        }

        private ToolInfo Downloader(ToolInfo info)
        {
            try
            {
                /// Note: do not use base WebClient, because it cannot 
                /// download multiple files concurrently.
                using var client = new WebClient();
                client.DownloadFile(
                    address: new Uri(
                        $"https://toolshed.g2.bx.psu.edu/repos/" +
                        $"{info.Tool.Owner}/{info.Tool.Name}/" +
                        $"archive/tip.zip"),
                    fileName: info.ArchiveFilename);
                return info;
            }
            catch (WebException e)
            {
                // TODO: log this exception.
                return null;
            }
        }

        private ToolInfo WrapperExtractor(ToolInfo info)
        {
            if (info == null)
                return null;

            try
            {
                using ZipArchive archive = ZipFile.Open(info.ArchiveFilename, ZipArchiveMode.Read);
                foreach (ZipArchiveEntry entry in archive.Entries)
                    if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        var extractedFileName = info.ArchiveExtractionPath + Path.GetFileName(entry.FullName);

                        /// Surrounding the file extraction from archive in a
                        /// try-catch block enables extracting XML files 
                        /// independently; hence, if one file is broken/invalid
                        /// the process can continue with other files that 
                        /// may be valid.
                        try
                        {
                            entry.ExtractToFile(extractedFileName);
                            info.XMLFiles.Add(extractedFileName);
                        }
                        catch (InvalidDataException e)
                        {
                            // This exception is thrown when the Zip archive cannot be read.
                            // TODO: log this.
                        }
                    }
                return info;
            }
            catch (InvalidDataException e)
            {
                // This exception is thrown when the Zip archive cannot be read.
                // TODO: log this.
                return null;
            }
        }

        private void ExtractPublications(ToolInfo info)
        {
            if (info == null)
                return;

            foreach (var filename in info.XMLFiles)
            {
                try
                {
                    XElement toolDoc = XElement.Load(filename);
                    var pubs = new List<Publication>();
                    foreach (var item in toolDoc.Elements("citations").Descendants())
                    {
                        if (item.Attribute("type") != null)
                        {
                            switch (item.Attribute("type").Value.Trim().ToUpperInvariant())
                            {
                                case "DOI":
                                    pubs.Add(new Publication() { DOI = item.Value });
                                    break;

                                case "BIBTEX":
                                    var parser = new Parser<Publication, Author, Keyword>(new PublicationConstructor(), new AuthorConstructor(), new KeywordConstructor());
                                    if (parser.TryParse(item.Value, out Publication pub))
                                        pubs.Add(pub);
                                    break;
                            }
                        }
                    }

                    TryAddEntities(info.Tool, pubs);
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
