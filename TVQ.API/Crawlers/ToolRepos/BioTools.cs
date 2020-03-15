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
                    typeof(Tool),
                    new BaseJsonConverter(
                        propertyMappings: new Dictionary<string, string>
                        {
                            { "name", nameof(Tool.Name) },
                            { "homepage", nameof(Tool.Homepage) },
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
                            { "owner", nameof(ToolRepoAssociation.Owner) },
                            { "user_id", nameof(ToolRepoAssociation.UserID) },
                            { "biotoolsID", nameof(ToolRepoAssociation.IDinRepo) },
                            { "additionDate", nameof(ToolRepoAssociation.DateAddedToRepository) }
                        }))
            };

            PublicationSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CustomContractResolver(
                    typeof(Publication),
                    new BaseJsonConverter(
                        propertyMappings: new Dictionary<string, string>
                        {
                            { "title", nameof(Publication.Title) }, 
                            { "year", nameof(Publication.Year) }, 
                            { "citedBy", nameof(Publication.CitedBy) }, 
                            { "doi", nameof(Publication.DOI) }, 
                            { "citation", nameof(Publication.BibTeXEntry) }, 
                            { "pmid", nameof(Publication.PubMedID) }
                        }))
            };

            CategorySerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CustomContractResolver(
                    typeof(CategoryRepoAssociation),
                    new CategoryRepoAssoJsonConverter())
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
                                PublicationSerializerSettings,
                                CategorySerializerSettings,
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
