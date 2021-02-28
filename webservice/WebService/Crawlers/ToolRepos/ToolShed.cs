using Genometric.TVQ.WebService.Crawlers.ToolRepos.HelperTypes;
using Genometric.TVQ.WebService.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.WebService.Model;
using Genometric.TVQ.WebService.Model.Associations;
using Genometric.TVQ.WebService.Model.JsonConverters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Genometric.TVQ.WebService.Crawlers.ToolRepos
{
    internal class ToolShed : BaseToolRepoCrawler
    {
        private const string _toolsFilename = "tools.json";
        private const string _categoriesFilename = "categories.json";
        private const string _publicationsFilename = "publications.json";

        private readonly int _maxParallelDownloads = 3;
        private readonly int _maxParallelActions = Environment.ProcessorCount * 3;
        private readonly int _boundedCapacity = Environment.ProcessorCount * 3;

        
        private readonly JsonSerializerSettings _categoryJsonSerializerSettings;

        public ToolShed(
            Repository repo,
            List<Tool> tools,
            List<Publication> publications,
            List<Category> categories,
            ILogger<BaseService<RepoCrawlingJob>> logger) :
            base(repo, tools, publications, categories, logger)
        {
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
            HttpResponseMessage response = client.GetAsync(new Uri(Repo.URI + _categoriesFilename)).GetAwaiter().GetResult();
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
            HttpResponseMessage response = await client.GetAsync(new Uri(Repo.URI + _toolsFilename)).ConfigureAwait(false);
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

        private async Task GetPublicationsAsync(List<DeserializedInfo> toolsInfo)
        {
            Logger.LogDebug("Getting tools list from ToolShed.");
            using var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(new Uri(Repo.URI + _publicationsFilename)).ConfigureAwait(false);
            string content;
            if (response.IsSuccessStatusCode)
                content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            else
                /// TODO: replace with an exception.
                return;

            var toolPublications = JsonConvert.DeserializeObject<Dictionary<string, List<Publication>>>(content);
            foreach (var publications in toolPublications)
            {
                // TODO: remove this check after the offline parser is updated. 
                if (publications.Value.Count > 0)
                {
                    foreach(var tool in toolsInfo)
                    {
                        if (tool.ToolRepoAssociation.IDinRepo == publications.Key)
                        {
                            var pubAssociations = new List<ToolPublicationAssociation>();
                            foreach (var pub in publications.Value)
                                pubAssociations.Add(new ToolPublicationAssociation()
                                {
                                    Publication = pub
                                });

                            tool.ToolPubAssociations = pubAssociations;
                            TryAddEntities(tool);
                            break;
                        }
                    }
                }
            }

            return;
        }
    }
}
