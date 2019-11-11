using Genometric.BibitemParser;
using Genometric.TVQ.API.Model;
using Microsoft.Extensions.Logging;
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

        private readonly ILogger<CrawlerService> _logger;

        public ToolShed(Repository repo, ILogger<CrawlerService> logger) : base(repo)
        {
            _logger = logger;

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
            _logger.LogDebug("Getting tools list from ToolShed.");
            HttpResponseMessage response = await HttpClient.GetAsync(new Uri(Repo.URI)).ConfigureAwait(false);
            string content;
            if (response.IsSuccessStatusCode)
                content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            else
                /// TODO: replace with an exception.
                return null;

            _logger.LogDebug("Received tools from ToolShed, deserializing them.");
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

            var extractPublications = new TransformBlock<ToolInfo, ToolInfo>(
                new Func<ToolInfo, ToolInfo>(ExtractPublications), _pubExtractExeOptions);

            var cleanup = new ActionBlock<ToolInfo>(
                input => { Cleanup(input); });

            downloader.LinkTo(extractXMLs, linkOptions);
            extractXMLs.LinkTo(extractPublications, linkOptions);
            extractPublications.LinkTo(cleanup, linkOptions);

            foreach (var tool in tools)
                downloader.Post(new ToolInfo(tool, SessionTempPath));
            downloader.Complete();

            await cleanup.Completion.ConfigureAwait(false);
        }

        private ToolInfo Downloader(ToolInfo info)
        {
            try
            {
                _logger.LogDebug($"Downloading archive of {info.Tool.Name}.");
                /// Note: do not use base WebClient, because it cannot 
                /// download multiple files concurrently.
                using var client = new WebClient();
                client.DownloadFile(
                    address: new Uri(
                        $"https://toolshed.g2.bx.psu.edu/repos/" +
                        $"{info.Tool.Owner}/{info.Tool.Name}/" +
                        $"archive/tip.zip"),
                    fileName: info.ArchiveFilename);
                _logger.LogDebug($"Successfully downloaded archive of {info.Tool.Name}.");
                return info;
            }
            catch (WebException e)
            {
                _logger.LogDebug($"Failed downloading archive of {info.Tool.Name}: {e.Message}");
                return null;
            }
        }

        private ToolInfo WrapperExtractor(ToolInfo info)
        {
            if (info == null)
                return null;

            try
            {
                _logger.LogDebug($"Extracting XML files from tool {info.Tool.Name} archive.");
                using ZipArchive archive = ZipFile.Open(info.ArchiveFilename, ZipArchiveMode.Read);
                foreach (ZipArchiveEntry entry in archive.Entries)
                    if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        /// A random string is appended to the filename to avoid filename 
                        /// collision when extracting and storing files with common names
                        /// in a common folder, which organized under different folders in 
                        /// an archive.
                        var extractedFileName = info.ArchiveExtractionPath +
                            Path.GetFileNameWithoutExtension(entry.FullName) + Utilities.GetRandomString(8);

                        /// Surrounding the file extraction from archive in a
                        /// try-catch block enables extracting XML files 
                        /// independently; hence, if one file is broken/invalid
                        /// the process can continue with other files that 
                        /// may be valid.
                        try
                        {
                            _logger.LogInformation($"Extracting XML file {entry.FullName} of tool {info.Tool.Name}.");
                            entry.ExtractToFile(extractedFileName);
                            _logger.LogInformation($"Successfully extracted XML file {entry.FullName} of tool {info.Tool.Name}.");
                            info.XMLFiles.Add(extractedFileName);
                        }
                        catch (InvalidDataException e)
                        {
                            // This exception is thrown when the Zip archive cannot be read.
                            _logger.LogDebug($"Failed extracting XML file {entry.FullName} of tool {info.Tool.Name}: {e.Message}");
                        }
                    }

                _logger.LogDebug($"Extracted {info.XMLFiles.Count} XML file(s) for tool {info.Tool.Name}.");
                return info;
            }
            catch (InvalidDataException e)
            {
                // This exception is thrown when the Zip archive cannot be read.
                _logger.LogDebug($"Failed extracting XML files from tool {info.Tool.Name} archive: {e.Message}");
                return null;
            }
        }

        private ToolInfo ExtractPublications(ToolInfo info)
        {
            if (info == null)
                return null;

            foreach (var filename in info.XMLFiles)
            {
                _logger.LogDebug(
                    $"Extracting publication info from XML file " +
                    $"{Path.GetFileNameWithoutExtension(filename)} " +
                    $"of tool {info.Tool.Name}.");

                try
                {
                    XElement toolDoc = XElement.Load(filename);
                    var pubs = new List<Publication>();
                    foreach (var item in toolDoc.Elements("citations").Descendants())
                        if (item.Attribute("type") != null)
                            switch (item.Attribute("type").Value.Trim().ToUpperInvariant())
                            {
                                case "DOI":
                                    pubs.Add(new Publication() { DOI = item.Value });
                                    /// Some tools have one BibItem that contains only DOI, and 
                                    /// another BibItem that contains publication info. There should
                                    /// be only one BibItem per publication contains both DOI and 
                                    /// publication info. Therefore, for tools with two bibitems,
                                    /// we consider only the one containing DOI. 
                                    continue;

                                case "BIBTEX":
                                    try
                                    {
                                        var parser = new Parser<Publication, Author, Keyword>(
                                            new PublicationConstructor(), 
                                            new AuthorConstructor(), 
                                            new KeywordConstructor());

                                        if (parser.TryParse(item.Value, out Publication pub) &&
                                            pub.Year != null)
                                            pubs.Add(pub);
                                    }
                                    catch (ArgumentException e)
                                    {
                                        _logger.LogDebug(
                                            $"Error extracting publication from XML file of tool " +
                                            $"{info.Tool.Name}:{e.Message}");
                                    }
                                    break;
                            }

                    _logger.LogDebug(
                        $"Successfully extract publication info from XML file " +
                        $"{Path.GetFileNameWithoutExtension(filename)} " +
                        $"of tool {info.Tool.Name}.");

                    TryAddEntities(info.Tool, pubs);
                }
                catch (System.Xml.XmlException e)
                {
                    /// This exception may happen if the XML 
                    /// file has multiple roots.
                    _logger.LogDebug(
                        $"Failed extracting publication info from XML file " +
                        $"{Path.GetFileNameWithoutExtension(filename)}" +
                        $" of tool {info.Tool.Name}: {e.Message}");

                    return null;
                }
            }

            return info;
        }

        private void Cleanup(ToolInfo info)
        {
            if (info == null)
                return;
            _logger.LogDebug($"Deleting temporary files of tool {info.Tool.Name}.");
            Directory.Delete(info.StagingArea, true);
            _logger.LogDebug($"Deleted temporary files of tool {info.Tool.Name}.");
        }
    }
}
