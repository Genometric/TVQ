using Genometric.BibitemParser;
using Genometric.BibitemParser.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;

namespace Genometric.TVQ.Crawlers.ToolShedCrawler
{
    public class Crawler
    {
        private const string _uri = "https://toolshed.g2.bx.psu.edu/api/";
        private const string _repositoriesEndpoint = "repositories";
        private const string _categoriesEndpoint = "categories";

        private string SessionTempPath { set; get; }

        private ILogger<Crawler> Logger { set; get; }

        private readonly int _maxParallelDownloads = 3;
        private readonly int _maxParallelActions = Environment.ProcessorCount * 3;
        private readonly int _boundedCapacity = Environment.ProcessorCount * 3;

        private ExecutionDataflowBlockOptions _downloadExeOptions;
        private ExecutionDataflowBlockOptions _xmlExtractExeOptions;
        private ExecutionDataflowBlockOptions _pubExtractExeOptions;

        private string _catogiresFilename;
        private string _toolsFilename;
        private string _publicationsFilename;

        private JsonSerializerSettings SerializerSettings { set; get; }
        private ConcurrentDictionary<string, List<Publication>> Publications { set; get; }
        private Parser BibitemParser { set; get; }

        public Crawler(ILogger<Crawler> logger)
        {
            Logger = logger;

            BibitemParser = new Parser();
            Publications = new ConcurrentDictionary<string, List<Publication>>();

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

            SerializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public void Crawl(string categoriesFilename, string toolsFilename, string publicationsFilename)
        {
            do
            {
                SessionTempPath =
                    Path.GetFullPath(Path.GetTempPath()) +
                    Utilities.GetRandomString(10) +
                    Path.DirectorySeparatorChar;
            }
            while (Directory.Exists(SessionTempPath));
            Directory.CreateDirectory(SessionTempPath);

            _catogiresFilename = categoriesFilename;
            _toolsFilename = toolsFilename;
            _publicationsFilename = publicationsFilename;

            Publications.Clear();

            UpdateCategories();
            var tools = GetTools().Result;
            if (tools != null)
            {
                foreach (var tool in tools)
                    if (!Publications.ContainsKey(tool.ID))
                        Publications.TryAdd(tool.ID, new List<Publication>());

                GetPublications(tools);
            }
        }

        private void UpdateCategories()
        {
            Logger.LogDebug("Getting Categories list from ToolShed.");
            using var client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(new Uri(_uri + _categoriesEndpoint)).GetAwaiter().GetResult();
            string content;
            if (response.IsSuccessStatusCode)
            {
                content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            else
            {
                Logger.LogCritical($"Failed getting categories from ToolShed; reason: {response.ReasonPhrase}");
                return;
            }

            Logger.LogDebug($"Serializing the received Categories to `{_catogiresFilename}`");
            WriteToJson(content, _catogiresFilename);
            Logger.LogInformation($"Serialized Categories to `{_catogiresFilename}`");
        }

        private async Task<List<Tool>> GetTools()
        {
            Logger.LogDebug("Getting Tools list from ToolShed.");
            using var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(new Uri(_uri + _repositoriesEndpoint)).ConfigureAwait(false);
            string content;
            if (response.IsSuccessStatusCode)
            {
                content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            else
            {
                Logger.LogCritical($"Failed getting categories from ToolShed; reason: {response.ReasonPhrase}");
                return null;
            }

            Logger.LogDebug($"Serializing the received Tools to `{_toolsFilename}`");
            WriteToJson(content, _toolsFilename);
            File.WriteAllText(_toolsFilename, content);
            Logger.LogInformation($"Serialized Tools to `{_toolsFilename}`");

            Logger.LogDebug($"Creating staging area per tool in: {SessionTempPath}");
            var tools = JsonConvert.DeserializeObject<List<Tool>>(content);
            foreach (var tool in tools)
                tool.EnsureStagingArea(SessionTempPath);
            Logger.LogDebug($"Created staging areas.");

            return tools;
        }

        /// <summary>
        /// This method is implemented using the Task Parallel Library (TPL).
        //. https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl
        /// </summary>
        private void GetPublications(List<Tool> tools)
        {
            Logger.LogDebug("Setting up TPL for downloading tools archives.");
            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            var downloader = new TransformBlock<Tool, Tool>(
                new Func<Tool, Tool>(Downloader), _downloadExeOptions);

            var extractXMLs = new TransformBlock<Tool, Tool>(
                new Func<Tool, Tool>(WrapperExtractor), _xmlExtractExeOptions);

            var extractPublications = new TransformBlock<Tool, Tool>(
                new Func<Tool, Tool>(ExtractPublications), _pubExtractExeOptions);

            var cleanup = new ActionBlock<Tool>(
                input => { Cleanup(input); });

            downloader.LinkTo(extractXMLs, linkOptions);
            extractXMLs.LinkTo(extractPublications, linkOptions);
            extractPublications.LinkTo(cleanup, linkOptions);

            Logger.LogDebug($"Starting to process (download archive, extract XML, extract citation) {tools.Count} tools *asynchronously*.");
            foreach (var info in tools)
                downloader.Post(info);

            downloader.Complete();

            cleanup.Completion.Wait();

            Logger.LogDebug($"Serializing extract bibliographies to `{_publicationsFilename}`");
            WriteToJson(JsonConvert.SerializeObject(Publications, SerializerSettings), _publicationsFilename);
            Logger.LogDebug($"Serialized extracted bibliographies.");
        }

        private Tool Downloader(Tool tool)
        {
            try
            {
                Logger.LogDebug($"Downloading archive of `{tool.Name}`.");

                /// Note: do not use base WebClient, because it cannot 
                /// download multiple files concurrently.
                using var client = new WebClient();
                client.DownloadFile(
                    address: new Uri(
                        $"https://toolshed.g2.bx.psu.edu/repos/" +
                        $"{tool.Owner}/{tool.Name}/" +
                        $"archive/tip.zip"),
                    fileName: tool.ArchiveFilename);
                Logger.LogDebug($"Successfully downloaded archive of `{tool.Name}`.");
                return tool;
            }
            catch (WebException e)
            {
                Logger.LogDebug($"Failed downloading archive of `{tool.Name}`: {e.Message}");
                return null;
            }
        }

        private Tool WrapperExtractor(Tool tool)
        {
            if (tool == null)
                return null;

            try
            {
                Logger.LogDebug($"Extracting XML files from tool `{tool.Name}` archive.");
                using ZipArchive archive = ZipFile.Open(tool.ArchiveFilename, ZipArchiveMode.Read);
                foreach (ZipArchiveEntry entry in archive.Entries)
                    if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        /// A random string is appended to the filename to avoid filename 
                        /// collision when extracting and storing files with common names
                        /// in a common folder, which organized under different folders in 
                        /// an archive.
                        var extractedFileName = tool.ArchiveExtractionPath +
                            Path.GetFileNameWithoutExtension(entry.FullName) + Utilities.GetRandomString(8);

                        /// Surrounding the file extraction from archive in a
                        /// try-catch block enables extracting XML files 
                        /// independently; hence, if one file is broken/invalid
                        /// the process can continue with other files that 
                        /// may be valid.
                        try
                        {
                            Logger.LogInformation($"Extracting XML file `{entry.FullName}` of tool `{tool.Name}`.");
                            entry.ExtractToFile(extractedFileName);
                            Logger.LogInformation($"Successfully extracted XML file `{entry.FullName}` of tool `{tool.Name}`.");
                            tool.XMLFiles.Add(extractedFileName);
                        }
                        catch (InvalidDataException e)
                        {
                            // This exception is thrown when the Zip archive cannot be read.
                            Logger.LogDebug($"Failed extracting XML file `{entry.FullName}` of tool `{tool.Name}`: {e.Message}");
                        }
                    }

                Logger.LogDebug($"Extracted `{tool.XMLFiles.Count}` XML file(s) for tool `{tool.Name}`.");
                return tool;
            }
            catch (InvalidDataException e)
            {
                // This exception is thrown when the Zip archive cannot be read.
                Logger.LogDebug($"Failed extracting XML files from tool `{tool.Name}` archive: {e.Message}");
                return null;
            }
        }

        private Tool ExtractPublications(Tool info)
        {
            if (info == null)
                return null;

            foreach (var filename in info.XMLFiles)
            {
                Logger.LogDebug(
                    $"Extracting publication info from XML file " +
                    $"`{Path.GetFileNameWithoutExtension(filename)}` " +
                    $"of tool `{info.Name}`.");

                try
                {
                    XElement toolDoc = XElement.Load(filename);
                    foreach (var item in toolDoc.Elements("citations").Descendants())
                    {
                        if (item.Attribute("type") != null)
                            switch (item.Attribute("type").Value.Trim().ToUpperInvariant())
                            {
                                case "DOI":
                                    Publications[info.ID].Add(new Publication() { DOI = item.Value });
                                    /// Some tools have one BibItem that contains only DOI, and 
                                    /// another BibItem that contains publication info. There should
                                    /// be only one BibItem per publication contains both DOI and 
                                    /// publication info. Therefore, for tools with two bibitems,
                                    /// we consider only the one containing DOI. 
                                    continue;

                                case "BIBTEX":
                                    try
                                    {
                                        if (TryParseBibitem(item.Value, out Publication pub))
                                            Publications[info.ID].Add(pub);
                                    }
                                    catch (ArgumentException e)
                                    {
                                        Logger.LogDebug(
                                            $"Error extracting publication from XML file of tool " +
                                            $"`{info.Name}`:{e.Message}");
                                    }
                                    break;
                            }
                    }

                    Logger.LogDebug(
                        $"Successfully extract publication info from XML file " +
                        $"`{Path.GetFileNameWithoutExtension(filename)}` " +
                        $"of tool `{info.Name}`.");
                }
                catch (System.Xml.XmlException e)
                {
                    /// This exception may happen if the XML 
                    /// file has multiple roots.
                    Logger.LogDebug(
                         $"Failed extracting publication info from XML file " +
                         $"`{Path.GetFileNameWithoutExtension(filename)}`" +
                         $" of tool `{info.Name}`: {e.Message}");

                    return null;
                }
            }

            return info;
        }

        private bool TryParseBibitem(string bibitem, out Publication publication)
        {
            if (BibitemParser.TryParse(bibitem, out Publication pub) &&
                (pub.DOI != null || pub.PubMedID != null || pub.Title != null))
            {
                publication = pub;
                return true;
            }
            else
            {
                publication = null;
                return false;
            }
        }

        private void WriteToJson(string content, string filename)
        {
            var json = JsonConvert.DeserializeObject<object>(content, SerializerSettings);
            using StreamWriter writer = new StreamWriter(filename);
            var serializer = JsonSerializer.Create(SerializerSettings);
            serializer.Serialize(writer, json);
        }

        private void Cleanup(Tool info)
        {
            if (info == null)
                return;
            Logger.LogDebug($"Deleting temporary files of tool {info.Name}.");
            Directory.Delete(info.StagingArea, true);
            Logger.LogDebug($"Deleted temporary files of tool {info.Name}.");
        }
    }
}
