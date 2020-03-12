using Genometric.TVQ.API.Crawlers.ToolRepos.HelperTypes;
using Genometric.TVQ.API.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.API.Model;
using Genometric.TVQ.API.Model.Associations;
using Genometric.TVQ.API.Model.JsonConverters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Crawlers.ToolRepos
{
    public class BioTools : BaseToolRepoCrawler
    {
        public BioTools(
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
                    typeof(Category),
                    new BaseJsonConverter(
                        propertyMappings: new Dictionary<string, string>
                        {
                            { "name", nameof(Tool.Name) },
                            { "homepage", nameof(Tool.Homepage) },
                            { "owner", nameof(Tool.Owner) },
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
                            { "user_id", nameof(ToolRepoAssociation.UserID) },
                            { "biotoolsID", nameof(ToolRepoAssociation.IDinRepo) },
                            { "additionDate", nameof(ToolRepoAssociation.DateAddedToRepository) }
                        }))
            };
        }

        public override async Task ScanAsync()
        {
            await DownloadFileAsync(Repo.GetURI(), out string archiveFileName).ConfigureAwait(false);
            TraverseArchive(archiveFileName);
        }

        private void TraverseArchive(string archiveFileName)
        {
            try
            {
                using ZipArchive archive = ZipFile.OpenRead(archiveFileName);
                foreach (ZipArchiveEntry entry in archive.Entries)
                    if (entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
                        !entry.FullName.EndsWith("oeb.json", StringComparison.OrdinalIgnoreCase))
                    {
                        string extractedFileName = SessionTempPath + Utilities.GetRandomString() + ".json";
                        try
                        {
                            entry.ExtractToFile(extractedFileName);
                            using var reader = new StreamReader(extractedFileName);
                            if (!DeserializedInfo.TryDeserialize(
                                reader.ReadToEnd(),
                                ToolJsonSerializerSettings,
                                ToolRepoAssoJsonSerializerSettings,
                                out DeserializedInfo deserializedInfo))
                            {
                                // TODO: log this.
                                continue;
                            }

                            if (!TryAddEntities(deserializedInfo))
                            {
                                // TODO: log why this tool will not be added to db.
                            }
                        }
                        catch (IOException e)
                        {
                            // TODO: log this.
                        }
                        finally
                        {
                            File.Delete(extractedFileName);
                        }
                    }
            }
            catch (Exception e)
            {
                // TODO: log the exception.
                // TODO: if this exception has occurred, the caller job's status should be set to failed.
            }
            finally
            {
                File.Delete(archiveFileName);
            }
        }
    }
}
