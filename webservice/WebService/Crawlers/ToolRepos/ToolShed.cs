using Genometric.TVQ.WebService.Crawlers.ToolRepos.HelperTypes;
using Genometric.TVQ.WebService.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.WebService.Model;
using Genometric.TVQ.WebService.Model.Associations;
using Genometric.TVQ.WebService.Model.JsonConverters;
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

namespace Genometric.TVQ.WebService.Crawlers.ToolRepos
{
    internal class ToolShed : BaseToolRepoCrawler
    {
        private const string _repositoriesEndpoint = "repositories";
        private const string _categoriesEndpoint = "categories";

        private readonly int _maxParallelDownloads = 3;
        private readonly int _maxParallelActions = Environment.ProcessorCount * 3;
        private readonly int _boundedCapacity = Environment.ProcessorCount * 3;

        private readonly ExecutionDataflowBlockOptions _downloadExeOptions;
        private readonly ExecutionDataflowBlockOptions _xmlExtractExeOptions;
        private readonly ExecutionDataflowBlockOptions _pubExtractExeOptions;

        
        private readonly JsonSerializerSettings _categoryJsonSerializerSettings;

        public ToolShed(
            Repository repo,
            List<Tool> tools,
            List<Publication> publications,
            List<Category> categories,
            ILogger<BaseService<RepoCrawlingJob>> logger) :
            base(repo, tools, publications, categories, logger)
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
        }

        public override async Task ScanAsync()
        {
            UpdateCategories();
            var tools = await GetToolsAsync().ConfigureAwait(false);
            if (tools != null)
                await GetPublicationsAsync(tools).ConfigureAwait(false);
        }

        private void UpdateCategories()
        {
            Logger.LogDebug("Getting Categories list from ToolShed.");
            using var client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(new Uri(Repo.URI + _categoriesEndpoint)).GetAwaiter().GetResult();
            string content;
            if (response.IsSuccessStatusCode)
                content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            else
                /// TODO: replace with an exception.
                return;

            Logger.LogDebug("Received Categories from ToolShed, deserializing them.");
            var associations = JsonConvert.DeserializeObject<List<CategoryRepoAssociation>>(
                content, _categoryJsonSerializerSettings);

            foreach (var association in associations)
                EnsureEntity(association);
        }

        private async Task<List<DeserializedInfo>> GetToolsAsync()
        {
            Logger.LogDebug("Getting tools list from ToolShed.");
            using var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(new Uri(Repo.URI + _repositoriesEndpoint)).ConfigureAwait(false);
            string content;
            if (response.IsSuccessStatusCode)
                content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            else
                /// TODO: replace with an exception.
                return null;

            Logger.LogDebug("Received tools from ToolShed, deserializing them.");
            DeserializedInfo.TryDeserialize(
                content, 
                ToolJsonSerializerSettings, 
                ToolRepoAssoJsonSerializerSettings, 
                out List<DeserializedInfo> deserializedInfos);
            foreach (var info in deserializedInfos)
                info.SetStagingArea(SessionTempPath);
            return deserializedInfos;
        }

        /// <summary>
        /// This method is implemented using the Task Parallel Library (TPL).
        //. https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl
        /// </summary>
        private async Task GetPublicationsAsync(List<DeserializedInfo> ToolsInfo)
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

            foreach (var info in ToolsInfo)
                downloader.Post(info);
            downloader.Complete();

            await cleanup.Completion.ConfigureAwait(false);
        }

        private DeserializedInfo Downloader(DeserializedInfo info)
        {
            try
            {
                Logger.LogDebug($"Downloading archive of {info.ToolRepoAssociation.Tool.Name}.");
                /// Note: do not use base WebClient, because it cannot 
                /// download multiple files concurrently.
                using var client = new WebClient();
                client.DownloadFile(
                    address: new Uri(
                        $"https://toolshed.g2.bx.psu.edu/repos/" +
                        $"{info.ToolRepoAssociation.Owner}/{info.ToolRepoAssociation.Tool.Name}/" +
                        $"archive/tip.zip"),
                    fileName: info.ArchiveFilename);
                Logger.LogDebug($"Successfully downloaded archive of {info.ToolRepoAssociation.Tool.Name}.");
                return info;
            }
            catch (WebException e)
            {
                Logger.LogDebug($"Failed downloading archive of {info.ToolRepoAssociation.Tool.Name}: {e.Message}");
                return null;
            }
        }

        private DeserializedInfo WrapperExtractor(DeserializedInfo info)
        {
            if (info == null)
                return null;

            try
            {
                Logger.LogDebug($"Extracting XML files from tool {info.ToolRepoAssociation.Tool.Name} archive.");
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
                            Logger.LogInformation($"Extracting XML file {entry.FullName} of tool {info.ToolRepoAssociation.Tool.Name}.");
                            entry.ExtractToFile(extractedFileName);
                            Logger.LogInformation($"Successfully extracted XML file {entry.FullName} of tool {info.ToolRepoAssociation.Tool.Name}.");
                            info.XMLFiles.Add(extractedFileName);
                        }
                        catch (InvalidDataException e)
                        {
                            // This exception is thrown when the Zip archive cannot be read.
                            Logger.LogDebug($"Failed extracting XML file {entry.FullName} of tool {info.ToolRepoAssociation.Tool.Name}: {e.Message}");
                        }
                    }

                Logger.LogDebug($"Extracted {info.XMLFiles.Count} XML file(s) for tool {info.ToolRepoAssociation.Tool.Name}.");
                return info;
            }
            catch (InvalidDataException e)
            {
                // This exception is thrown when the Zip archive cannot be read.
                Logger.LogDebug($"Failed extracting XML files from tool {info.ToolRepoAssociation.Tool.Name} archive: {e.Message}");
                return null;
            }
        }

        private DeserializedInfo ExtractPublications(DeserializedInfo info)
        {
            if (info == null)
                return null;

            foreach (var filename in info.XMLFiles)
            {
                Logger.LogDebug(
                    $"Extracting publication info from XML file " +
                    $"{Path.GetFileNameWithoutExtension(filename)} " +
                    $"of tool {info.ToolRepoAssociation.Tool.Name}.");

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
                                    pubAssociations.Add(
                                        new ToolPublicationAssociation()
                                        {
                                            Publication = new Publication()
                                            {
                                                DOI = item.Value
                                            }
                                        });
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
                                            pubAssociations.Add(
                                                new ToolPublicationAssociation() { Publication = pub });
                                    }
                                    catch (ArgumentException e)
                                    {
                                        Logger.LogDebug(
                                            $"Error extracting publication from XML file of tool " +
                                            $"{info.ToolRepoAssociation.Tool.Name}:{e.Message}");
                                    }
                                    break;
                            }
                    }

                    Logger.LogDebug(
                        $"Successfully extract publication info from XML file " +
                        $"{Path.GetFileNameWithoutExtension(filename)} " +
                        $"of tool {info.ToolRepoAssociation.Tool.Name}.");

                    info.ToolPubAssociations = pubAssociations;
                    TryAddEntities(info);
                }
                catch (System.Xml.XmlException e)
                {
                    /// This exception may happen if the XML 
                    /// file has multiple roots.
                    Logger.LogDebug(
                        $"Failed extracting publication info from XML file " +
                        $"{Path.GetFileNameWithoutExtension(filename)}" +
                        $" of tool {info.ToolRepoAssociation.Tool.Name}: {e.Message}");

                    return null;
                }
            }

            return info;
        }

        private void Cleanup(DeserializedInfo info)
        {
            if (info == null)
                return;
            Logger.LogDebug($"Deleting temporary files of tool {info.ToolRepoAssociation.Tool.Name}.");
            Directory.Delete(info.StagingArea, true);
            Logger.LogDebug($"Deleted temporary files of tool {info.ToolRepoAssociation.Tool.Name}.");
        }
    }
}
