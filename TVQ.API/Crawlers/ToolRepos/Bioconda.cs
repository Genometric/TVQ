using Genometric.TVQ.API.Crawlers.ToolRepos.HelperTypes;
using Genometric.TVQ.API.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Genometric.TVQ.API.Crawlers.ToolRepos
{
    public class Bioconda : BaseToolRepoCrawler
    {
        private readonly string _dateAddedFileName = "bioconda_recipes_add_date.txt";

        private Dictionary<string, DateTime?> _addedDates;

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
            GetAddDate();
            await DownloadFileAsync(Repo.GetURI(), out string archiveFileName).ConfigureAwait(false);
            TraverseArchive(archiveFileName);
        }

        private void GetAddDate()
        {
            /// Why this info is not directly fetched from the Bioconda's repository?
            /// There is separate script in the Scripts folder that retrieves the date 
            /// when each tool was added to the Bioconda's repository from the repository's 
            /// git log. The algorithm to retrieve the dates is not optimized, hence it
            /// takes significantly long time (in order of hours) to retrieve such info.
            /// Therefore, until the algorithm is optimized and a better code is implemented 
            /// here (maybe using LibGit2Sharp (https://github.com/libgit2/libgit2sharp)), 
            /// it is more practical to run the afore-mentioned script offline, cache the 
            /// date and store them in the TVQ's git repository, and then read that file 
            /// in this method (how it is currently implemented).

            var dateAddedFileName = SessionTempPath + Utilities.GetRandomString();

            using var client = new WebClient();
            client.DownloadFileTaskAsync("https://github.com/Genometric/TVQ/raw/master/data/bioconda/"
                                         + _dateAddedFileName, dateAddedFileName).Wait();

            _addedDates = new Dictionary<string, DateTime?>();

            using var reader = new StreamReader(dateAddedFileName);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var columns = line.Split('\t');
                if (!_addedDates.ContainsKey(columns[0]))
                {
                    DateTimeOffset.TryParseExact(
                        columns[1].Trim(),
                        "ddd MMM d HH:mm:ss yyyy K",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTimeOffset dateTimeOffset);

                    _addedDates.Add(columns[0].Trim(), dateTimeOffset.DateTime);
                }
            }
        }

        private void TraverseArchive(string archiveFileName)
        {
            try
            {
                using ZipArchive archive = ZipFile.OpenRead(archiveFileName);
                foreach (ZipArchiveEntry entry in archive.Entries)
                    if (entry.FullName.EndsWith("meta.yaml", StringComparison.OrdinalIgnoreCase))
                    {
                        string extractedFileName = SessionTempPath + Utilities.GetRandomString() + ".yaml";
                        try
                        {
                            entry.ExtractToFile(extractedFileName);
                            using var reader = new StreamReader(extractedFileName);
                            var yaml = new YamlStream();
                            yaml.Load(reader);
                            if (!DeserializedInfo.TryDeserialize(yaml, out DeserializedInfo deserializedInfo))
                            {
                                // TODO: log this.
                                continue;
                            }

                            if (deserializedInfo.ToolRepoAssociation != null &&
                                deserializedInfo.ToolRepoAssociation.Tool != null &&
                                deserializedInfo.ToolRepoAssociation.Tool.Name != null &&
                                _addedDates.ContainsKey(deserializedInfo.ToolRepoAssociation.Tool.Name))
                                deserializedInfo.ToolRepoAssociation.DateAddedToRepository =
                                    _addedDates[deserializedInfo.ToolRepoAssociation.Tool.Name];

                            if (!TryAddEntities(deserializedInfo))
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
