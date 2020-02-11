using Genometric.TVQ.API.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Genometric.TVQ.API.Crawlers.ToolRepos
{
    public class Bioconda : BaseToolRepoCrawler
    {
        public Bioconda(Repository repository,
                        List<Tool> tools,
                        List<Publication> publications,
                        List<Category> categories) :
            base(repository,
                 tools,
                 publications,
                 categories)
        { }

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
                    if (entry.FullName.EndsWith("meta.yaml", StringComparison.OrdinalIgnoreCase))
                    {
                        ToolRepoAssociation toolRepoAssociation = null;
                        List<ToolPublicationAssociation> toolPubAssociations = null;

                        string extractedFileName = SessionTempPath + Utilities.GetRandomString() + ".yaml";
                        try
                        {
                            entry.ExtractToFile(extractedFileName);
                            using var reader = new StreamReader(extractedFileName);
                            var yaml = new YamlStream();
                            yaml.Load(reader);
                            if (!RepoTool.TryDeserialize(yaml,
                                                         out toolRepoAssociation,
                                                         out toolPubAssociations))
                            {
                                // TODO: log this.
                                continue;
                            }

                            if (!TryAddEntities(toolRepoAssociation, toolPubAssociations))
                            {
                                // TODO: log why this tool will not be added to db.
                            }
                        }
                        catch (IOException e)
                        {
                            // TODO: log this.
                        }
                        catch (YamlDotNet.Core.SyntaxErrorException e)
                        {
                            // TODO: log as malformed YAML file. 
                        }
                        catch (YamlDotNet.Core.SemanticErrorException e)
                        {
                            // TODO: log as malformed YAML file. 
                        }
                        catch (YamlDotNet.Core.YamlException e)
                        {
                            // TODO: log as malformed YAML file.
                        }
                        finally
                        {
                            File.Delete(extractedFileName);
                        }
                    }
            }
            catch (Exception e)
            {

            }
            finally
            {
                File.Delete(archiveFileName);
            }
        }
    }
}
