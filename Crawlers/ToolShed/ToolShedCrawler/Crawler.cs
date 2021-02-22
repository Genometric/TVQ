using Autofac;
using Autofac.Extensions.DependencyInjection;
using Genometric.BibitemParser;
using Genometric.TVQ.WebService.Model;
using Genometric.TVQ.WebService.Model.Associations;
using Genometric.TVQ.WebService.Model.JsonConverters;
using Microsoft.Extensions.DependencyInjection;
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

namespace ToolShedCrawler
{
    class Crawler
    {
        private const string _uri = "https://toolshed.g2.bx.psu.edu/api/";
        private const string _repositoriesEndpoint = "repositories";
        private const string _categoriesEndpoint = "categories";

        private static string SessionTempPath { set; get; }

        private static ILogger<Crawler> Logger { set; get; }

        private static readonly int _maxParallelDownloads = 3;
        private static readonly int _maxParallelActions = Environment.ProcessorCount * 3;
        private static readonly int _boundedCapacity = Environment.ProcessorCount * 3;

        private static ExecutionDataflowBlockOptions _downloadExeOptions;
        private static ExecutionDataflowBlockOptions _xmlExtractExeOptions;
        private static ExecutionDataflowBlockOptions _pubExtractExeOptions;

        private static string CatogiresFilename { get; } = "Categories.json";
        private static string ToolsFilename { get; } = "Tools.json";
        private static string PublicationsFilename { get; } = "Publications.json";
        private static JsonSerializerSettings SerializerSettings { set; get; }
        private static ConcurrentDictionary<string, List<Publication>> Publications { set; get; }
        private static Parser<Publication, Author, Keyword> BibitemParser { set; get; }
        private static JsonSerializerSettings CategoryJsonSerializerSettings { set; get; }

        private static void ConfigureLogging(ILoggingBuilder log)
        {
            log.ClearProviders();
            log.SetMinimumLevel(LogLevel.Error);
            log.AddConsole();
        }

        private static void ConfigureContainer(ContainerBuilder builder)
        {
            builder.Register(handler => LoggerFactory.Create(ConfigureLogging))
                .As<ILoggerFactory>()
                .SingleInstance()
                .AutoActivate();

            builder.RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>))
                .SingleInstance();
        }

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


            var containerBuilder = new ContainerBuilder();
            ConfigureContainer(containerBuilder);

            var container = containerBuilder.Build();
            var serviceProvider = new AutofacServiceProvider(container);
            Logger = serviceProvider.GetService<ILogger<Crawler>>();

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

            CategoryJsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CustomContractResolver(
                    typeof(CategoryRepoAssociation),
                    new CategoryRepoAssoJsonConverter())
            };

            BibitemParser = new Parser<Publication, Author, Keyword>(
                new PublicationConstructor(),
                new AuthorConstructor(),
                new KeywordConstructor());

            Publications = new ConcurrentDictionary<string, List<Publication>>();

            SerializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

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

        private static void UpdateCategories()
        {
            Logger.LogDebug("Getting Categories list from ToolShed.");
            using var client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(new Uri(_uri + _categoriesEndpoint)).GetAwaiter().GetResult();
            string content;
            if (response.IsSuccessStatusCode)
                content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            else
                /// TODO: replace with an exception.
                return;

            Logger.LogDebug("Received Categories from ToolShed, deserializing them.");
            var associations = JsonConvert.DeserializeObject<List<CategoryRepoAssociation>>(
                content, CategoryJsonSerializerSettings);

            WriteToJson(content, CatogiresFilename);
        }

        private static async Task<List<Tool>> GetTools()
        {
            using var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(new Uri(_uri + _repositoriesEndpoint)).ConfigureAwait(false);
            string content;
            if (response.IsSuccessStatusCode)
                content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            else
                /// TODO: replace with an exception.
                return null;

            WriteToJson(content, ToolsFilename);
            File.WriteAllText(ToolsFilename, content);

            var tools = JsonConvert.DeserializeObject<List<Tool>>(content);
            foreach (var tool in tools)
                tool.EnsureStagingArea(SessionTempPath);

            return tools;
        }

        private static void WriteToJson(string content, string filename)
        {
            var json = JsonConvert.DeserializeObject<object>(content, SerializerSettings);
            using StreamWriter writer = new StreamWriter(filename);
            var serializer = JsonSerializer.Create(SerializerSettings);
            serializer.Serialize(writer, json);
        }

        /// <summary>
        /// This method is implemented using the Task Parallel Library (TPL).
        //. https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl
        /// </summary>
        private static void GetPublications(List<Tool> tools)
        {
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

            foreach (var info in tools)
                downloader.Post(info);

            downloader.Complete();

            cleanup.Completion.Wait();

            WriteToJson(JsonConvert.SerializeObject(Publications, SerializerSettings), PublicationsFilename);
        }

        private static Tool Downloader(Tool info)
        {
            try
            {
                /// Note: do not use base WebClient, because it cannot 
                /// download multiple files concurrently.
                using var client = new WebClient();
                client.DownloadFile(
                    address: new Uri(
                        $"https://toolshed.g2.bx.psu.edu/repos/" +
                        $"{info.Owner}/{info.Name}/" +
                        $"archive/tip.zip"),
                    fileName: info.ArchiveFilename);
                Logger.LogDebug($"Successfully downloaded archive of {info.Name}.");
                return info;
            }
            catch (WebException e)
            {
                Logger.LogDebug($"Failed downloading archive of {info.Name}: {e.Message}");
                return null;
            }
        }

        private static Tool WrapperExtractor(Tool info)
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

        private static Tool ExtractPublications(Tool info)
        {
            if (info == null)
                return null;

            foreach (var filename in info.XMLFiles)
            {
                Logger.LogDebug(
                    $"Extracting publication info from XML file " +
                    $"{Path.GetFileNameWithoutExtension(filename)} " +
                    $"of tool {info.Name}.");

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
                                        {
                                            Publications[info.ID].Add(pub);
                                        }
                                    }
                                    catch (ArgumentException e)
                                    {
                                        Logger.LogDebug(
                                            $"Error extracting publication from XML file of tool " +
                                            $"{info.Name}:{e.Message}");
                                    }
                                    break;
                            }
                    }

                    Logger.LogDebug(
                        $"Successfully extract publication info from XML file " +
                        $"{Path.GetFileNameWithoutExtension(filename)} " +
                        $"of tool {info.Name}.");
                }
                catch (System.Xml.XmlException e)
                {
                    /// This exception may happen if the XML 
                    /// file has multiple roots.
                    Logger.LogDebug(
                         $"Failed extracting publication info from XML file " +
                         $"{Path.GetFileNameWithoutExtension(filename)}" +
                         $" of tool {info.Name}: {e.Message}");

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

        private static void Cleanup(Tool info)
        {
            if (info == null)
                return;
            Logger.LogDebug($"Deleting temporary files of tool {info.Name}.");
            Directory.Delete(info.StagingArea, true);
            Logger.LogDebug($"Deleted temporary files of tool {info.Name}.");
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole()).AddTransient<Crawler>();
        }
    }
}
