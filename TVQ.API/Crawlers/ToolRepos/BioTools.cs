﻿using Genometric.TVQ.API.Model;
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
            List<Category> categories) :
            base(repo, tools, publications, categories)
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
                    if (entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
                        !entry.FullName.EndsWith("oeb.json", StringComparison.OrdinalIgnoreCase))
                    {
                        string extractedFileName = SessionTempPath + Utilities.GetRandomString() + ".json";
                        try
                        {
                            entry.ExtractToFile(extractedFileName);
                            using var reader = new StreamReader(extractedFileName);
                            if (!RepoTool.TryDeserialize(reader.ReadToEnd(),
                                                         SessionTempPath,
                                                         out ToolInfo toolInfo))
                            {
                                // TODO: log this.
                                continue;
                            }

                            if (!TryAddEntities(toolInfo))
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
