using Genometric.TVQ.WebService.Crawlers.ToolRepos.HelperTypes;
using Genometric.TVQ.WebService.Model;
using Genometric.TVQ.WebService.Model.Associations;
using Genometric.TVQ.WebService.Model.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Genometric.BibitemParser;
using System.Xml.Linq;
using System.Collections.Concurrent;

namespace ToolShedCrawler
{
    class Crawler
    {
        private const string _uri = "https://toolshed.g2.bx.psu.edu/api/";
        private const string _repositoriesEndpoint = "repositories";
        private const string _categoriesEndpoint = "categories";

        private static string SessionTempPath { set; get; }

        private static int _maxParallelDownloads = 1;//3;
        private static int _maxParallelActions = 1;//Environment.ProcessorCount * 3;
        private static int _boundedCapacity = 1;//Environment.ProcessorCount * 3;

        private static JsonSerializerSettings ToolJsonSerializerSettings { set; get; }
        private static JsonSerializerSettings ToolRepoAssoJsonSerializerSettings { set; get; }
        private static JsonSerializerSettings PublicationSerializerSettings { set; get; }
        private static JsonSerializerSettings CategorySerializerSettings { set; get; }
        private static JsonSerializerSettings _categoryJsonSerializerSettings { set; get; }

        private static ExecutionDataflowBlockOptions _downloadExeOptions;
        private static ExecutionDataflowBlockOptions _xmlExtractExeOptions;
        private static ExecutionDataflowBlockOptions _pubExtractExeOptions;

        private static ConcurrentDictionary<string, List<Publication>> _publications { set; get; }

        private static Parser<Publication, Author, Keyword> BibitemParser { set; get; }

        private static string _catogiresFilename { get; } = "Categories.json";
        private static string _toolsFilename { get; } = "Tools.json";
        private static string _publicationsFilename { get; } = "Publications.json";


        static void Main(string[] args)
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

            ToolJsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CustomContractResolver(
                    typeof(Tool),
                    new BaseJsonConverter(
                        propertyMappings: new Dictionary<string, string>
                        {
                            { "name", nameof(Tool.Name) },
                            { "homepage_url", nameof(Tool.Homepage) },
                            { "remote_repository_url", nameof(Tool.CodeRepo) },
                            { "description", nameof(Tool.Description) }
                        }))
            };

            ToolRepoAssoJsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CustomContractResolver(
                    typeof(ToolRepoAssociation),
                    new BaseJsonConverter(
                        propertyMappings: new Dictionary<string, string>
                        {
                            { "times_downloaded", nameof(ToolRepoAssociation.TimesDownloaded) },
                            { "owner", nameof(ToolRepoAssociation.Owner) },
                            { "user_id", nameof(ToolRepoAssociation.UserID) },
                            { "id", nameof(ToolRepoAssociation.IDinRepo) },
                            { "create_time", nameof(ToolRepoAssociation.DateAddedToRepository) }
                        }))
            };

            _categoryJsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CustomContractResolver(
                    typeof(CategoryRepoAssociation),
                    new CategoryRepoAssoJsonConverter())
            };

            BibitemParser = new Parser<Publication, Author, Keyword>(
                new PublicationConstructor(),
                new AuthorConstructor(),
                new KeywordConstructor());

            _publications = new ConcurrentDictionary<string, List<Publication>>();

            UpdateCategories();
            var tools = GetTools().Result;
            if (tools != null)
            {
                foreach (var tool in tools)
                {
                    var toolID = tool.ToolRepoAssociation.IDinRepo;
                    if (!_publications.ContainsKey(toolID))
                        _publications.TryAdd(toolID, new List<Publication>());
                }
                GetPublications(tools);
            }
        }

        private static void UpdateCategories()
        {
            //Logger.LogDebug("Getting Categories list from ToolShed.");
            using var client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(new Uri(_uri + _categoriesEndpoint)).GetAwaiter().GetResult();
            string content;
            if (response.IsSuccessStatusCode)
                content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            else
                /// TODO: replace with an exception.
                return;

            //Logger.LogDebug("Received Categories from ToolShed, deserializing them.");
            var associations = JsonConvert.DeserializeObject<List<CategoryRepoAssociation>>(
                content, _categoryJsonSerializerSettings);

            WriteToJson(content, _catogiresFilename);
        }

        private static async Task<List<DeserializedInfo>> GetTools()
        {
            using var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(new Uri(_uri + _repositoriesEndpoint)).ConfigureAwait(false);
            string content;
            if (response.IsSuccessStatusCode)
                content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            else
                /// TODO: replace with an exception.
                return null;

            WriteToJson(content, _toolsFilename);
            File.WriteAllText(_toolsFilename, content);

            DeserializedInfo.TryDeserialize(
                content,
                ToolJsonSerializerSettings,
                ToolRepoAssoJsonSerializerSettings,
                out List<DeserializedInfo> deserializedInfos);
            foreach (var info in deserializedInfos)
                info.SetStagingArea(SessionTempPath);
            return deserializedInfos;
        }

        private static void WriteToJson(string content, string filename)
        {
            var json = JsonConvert.DeserializeObject<object>(content);
            using StreamWriter writer = new StreamWriter(filename);
            JsonSerializer serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented
            };
            serializer.Serialize(writer, json);
        }

        /// <summary>
        /// This method is implemented using the Task Parallel Library (TPL).
        //. https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl
        /// </summary>
        private static void GetPublications(List<DeserializedInfo> ToolsInfo)
        {
            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            var downloader = new TransformBlock<DeserializedInfo, DeserializedInfo>(
                new Func<DeserializedInfo, DeserializedInfo>(Downloader), _downloadExeOptions);

            var extractXMLs = new TransformBlock<DeserializedInfo, DeserializedInfo>(
                new Func<DeserializedInfo, DeserializedInfo>(WrapperExtractor), _xmlExtractExeOptions);

            var extractPublications = new TransformBlock<DeserializedInfo, DeserializedInfo>(
                new Func<DeserializedInfo, DeserializedInfo>(ExtractPublications), _pubExtractExeOptions);

            var cleanup = new ActionBlock<DeserializedInfo>(
                input => { Cleanup(input); });

            downloader.LinkTo(extractXMLs, linkOptions);
            extractXMLs.LinkTo(extractPublications, linkOptions);
            extractPublications.LinkTo(cleanup, linkOptions);

            var temp = 0;
            foreach (var info in ToolsInfo)
            {
                temp++;
                if (temp > 10)
                    break;
                downloader.Post(info);
            }
            downloader.Complete();

            cleanup.Completion.Wait();

            var json = JsonConvert.SerializeObject(_publications);
            WriteToJson(json, _publicationsFilename);
        }

        private static DeserializedInfo Downloader(DeserializedInfo info)
        {
            try
            {
                /// Note: do not use base WebClient, because it cannot 
                /// download multiple files concurrently.
                using var client = new WebClient();
                client.DownloadFile(
                    address: new Uri(
                        $"https://toolshed.g2.bx.psu.edu/repos/" +
                        $"{info.ToolRepoAssociation.Owner}/{info.ToolRepoAssociation.Tool.Name}/" +
                        $"archive/tip.zip"),
                    fileName: info.ArchiveFilename);
                //Logger.LogDebug($"Successfully downloaded archive of {info.ToolRepoAssociation.Tool.Name}.");
                return info;
            }
            catch (WebException e)
            {
                //Logger.LogDebug($"Failed downloading archive of {info.ToolRepoAssociation.Tool.Name}: {e.Message}");
                return null;
            }
        }

        private static DeserializedInfo WrapperExtractor(DeserializedInfo info)
        {
            if (info == null)
                return null;

            try
            {
                //Logger.LogDebug($"Extracting XML files from tool {info.ToolRepoAssociation.Tool.Name} archive.");
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
                            //Logger.LogInformation($"Extracting XML file {entry.FullName} of tool {info.ToolRepoAssociation.Tool.Name}.");
                            entry.ExtractToFile(extractedFileName);
                            //Logger.LogInformation($"Successfully extracted XML file {entry.FullName} of tool {info.ToolRepoAssociation.Tool.Name}.");
                            info.XMLFiles.Add(extractedFileName);
                        }
                        catch (InvalidDataException e)
                        {
                            // This exception is thrown when the Zip archive cannot be read.
                            //Logger.LogDebug($"Failed extracting XML file {entry.FullName} of tool {info.ToolRepoAssociation.Tool.Name}: {e.Message}");
                        }
                    }

                //Logger.LogDebug($"Extracted {info.XMLFiles.Count} XML file(s) for tool {info.ToolRepoAssociation.Tool.Name}.");
                return info;
            }
            catch (InvalidDataException e)
            {
                // This exception is thrown when the Zip archive cannot be read.
                //Logger.LogDebug($"Failed extracting XML files from tool {info.ToolRepoAssociation.Tool.Name} archive: {e.Message}");
                return null;
            }
        }

        private static DeserializedInfo ExtractPublications(DeserializedInfo info)
        {
            if (info == null)
                return null;

            foreach (var filename in info.XMLFiles)
            {
                /*Logger.LogDebug(
                    $"Extracting publication info from XML file " +
                    $"{Path.GetFileNameWithoutExtension(filename)} " +
                    $"of tool {info.ToolRepoAssociation.Tool.Name}.");*/

                try
                {
                    XElement toolDoc = XElement.Load(filename);
                    var pubAssociations = new List<ToolPublicationAssociation>();
                    foreach (var item in toolDoc.Elements("citations").Descendants())
                    {
                        if (item.Attribute("type") != null)
                            switch (item.Attribute("type").Value.Trim().ToUpperInvariant())
                            {
                                case "DOI":
                                    _publications[info.ToolRepoAssociation.IDinRepo].Add(new Publication() { DOI = item.Value });
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
                                        {
                                            _publications[info.ToolRepoAssociation.IDinRepo].Add(pub);
                                        }
                                    }
                                    catch (ArgumentException e)
                                    {
                                        /*Logger.LogDebug(
                                            $"Error extracting publication from XML file of tool " +
                                            $"{info.ToolRepoAssociation.Tool.Name}:{e.Message}");*/
                                    }
                                    break;
                            }
                    }

                    /*Logger.LogDebug(
                        $"Successfully extract publication info from XML file " +
                        $"{Path.GetFileNameWithoutExtension(filename)} " +
                        $"of tool {info.ToolRepoAssociation.Tool.Name}.");*/

                    info.ToolPubAssociations = pubAssociations;
                    //TryAddEntities(info);
                }
                catch (System.Xml.XmlException e)
                {
                    /// This exception may happen if the XML 
                    /// file has multiple roots.
                   /* Logger.LogDebug(
                        $"Failed extracting publication info from XML file " +
                        $"{Path.GetFileNameWithoutExtension(filename)}" +
                        $" of tool {info.ToolRepoAssociation.Tool.Name}: {e.Message}");*/

                    return null;
                }
            }

            return info;
        }

        private static bool TryParseBibitem(string bibitem, out Publication publication)
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

        private static void Cleanup(DeserializedInfo info)
        {
            if (info == null)
                return;
            //Logger.LogDebug($"Deleting temporary files of tool {info.ToolRepoAssociation.Tool.Name}.");
            Directory.Delete(info.StagingArea, true);
            //Logger.LogDebug($"Deleted temporary files of tool {info.ToolRepoAssociation.Tool.Name}.");
        }
    }
}
