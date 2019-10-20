using Genometric.TVQ.API.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Crawlers
{
    public class BioTools : ToolRepoCrawler
    {
        public BioTools(Repository repo, List<Tool> tools) : base(repo, tools)
        { }

        public override async Task ScanAsync()
        {
            var archiveFileName = DownloadArchive();
            await TraverseArchive(archiveFileName).ConfigureAwait(false);
        }

        private string DownloadArchive()
        {
            var archiveFileName = SessionTempPath + Utilities.GetRandomString();
            WebClient.DownloadFileTaskAsync(Repo.GetURI(), archiveFileName).Wait();
            return archiveFileName;
        }

        private async Task TraverseArchive(string archiveFileName)
        {
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(archiveFileName))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                        if (entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
                            !entry.FullName.EndsWith("oeb.json"))
                        {
                            string extractedFileName = SessionTempPath + Utilities.GetRandomString() + ".json";
                            entry.ExtractToFile(extractedFileName);
                            var tool = ExtractTool(extractedFileName);
                            var pubs = ExtractPublications(extractedFileName);

                            TryAddEntities(tool, pubs);

                            File.Delete(extractedFileName);
                        }
                }
            }
            catch (Exception e)
            {
                // TODO: log the exception.
            }
        }

        private Tool ExtractTool(string fileName)
        {
            using (StreamReader r = new StreamReader(fileName))
                return JsonConvert.DeserializeObject<Tool>(r.ReadToEnd());
        }

        private List<Publication> ExtractPublications(string toolFileName)
        {
            var pubs = new List<Publication>();
            using (StreamReader r = new StreamReader(toolFileName))
            {
                dynamic array = JsonConvert.DeserializeObject(r.ReadToEnd());
                foreach (JProperty jProperty in array)
                {
                    if (jProperty.Name == "publication")
                    {
                        foreach (JObject jPub in jProperty.Value)
                            pubs.Add(JsonConvert.DeserializeObject<Publication>(jPub.ToString()));
                        break;
                    }
                }
            }
            return pubs;
        }
    }
}
