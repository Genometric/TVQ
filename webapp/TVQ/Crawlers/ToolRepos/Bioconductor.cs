using Genometric.TVQ.API.Crawlers.ToolRepos.HelperTypes;
using Genometric.TVQ.API.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.API.Model;
using Genometric.TVQ.API.Model.Associations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Crawlers.ToolRepos
{
    public class Bioconductor : BaseToolRepoCrawler
    {
        private readonly string _citationsFileName = "citations.json";
        private readonly string _statsFileName = "package_stats.tsv";
        private readonly string _dateAddedFileName = "first_appearance.csv";
        private readonly string _biocViews = "biocViews.json";

        private Dictionary<string, DateTime?> _toolsAddToRepoDates;
        private Dictionary<string, List<CategoryRepoAssociation>> _categoryRepoAssociations;

        public Bioconductor(
            Repository repo,
            List<Tool> tools,
            List<Publication> publications,
            List<Category> categories,
            ILogger<BaseService<RepoCrawlingJob>> logger) :
            base(repo, tools, publications, categories, logger)
        {
            BibitemParser.KeywordsDelimiter = ',';
        }

        public override async Task ScanAsync()
        {
            GetAddedDate();
            GetBiocView();
            ReadCitationsFile();
            GetDownloadStats();
        }

        private void GetBiocView()
        {
            Logger.LogDebug("Downloading BiocView file.");
            var biocViewsFilename = SessionTempPath + Utilities.GetRandomString();
            using var client = new WebClient();
            client.DownloadFileTaskAsync(Repo.GetURI() + _biocViews, biocViewsFilename).Wait();

            Logger.LogDebug("Downloaded BiocView file, deserializing biocView per tool.");
            using (var reader = new StreamReader(biocViewsFilename))
            {
                string json = reader.ReadToEnd();
                var biocViews = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
                _categoryRepoAssociations = new Dictionary<string, List<CategoryRepoAssociation>>();
                foreach (var view in biocViews)
                {
                    var categories = new List<CategoryRepoAssociation>();
                    foreach (var item in view.Value)
                    {
                        if (string.IsNullOrWhiteSpace(item))
                            continue;

                        categories.Add(new CategoryRepoAssociation() { Category = new Category() { Name = item.Trim() } });
                    }
                    _categoryRepoAssociations.TryAdd(view.Key.Trim(), categories);
                }
            }

            File.Delete(biocViewsFilename);
            Logger.LogDebug("Completed biocView deserialization.");
        }

        private void ReadCitationsFile()
        {
            var citationsFileName = SessionTempPath + Utilities.GetRandomString();

            /// Use a new WebClient instance for downloads, because 
            /// an instance of WebClient does not support concurrent
            /// downloads. 
            using var client = new WebClient();
            client.DownloadFileTaskAsync(Repo.GetURI() + _citationsFileName, citationsFileName).Wait();

            using (var reader = new StreamReader(citationsFileName))
            {
                string json = reader.ReadToEnd();
                var items = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                foreach (var item in items)
                    try
                    {
                        if (TryParseBibitem(item.Value, out Publication pub))
                        {
                            var toolName = item.Key.Trim();
                            _toolsAddToRepoDates.TryGetValue(
                                toolName,
                                out DateTime? addedDate); ;

                            _categoryRepoAssociations.TryGetValue(
                                toolName,
                                out List<CategoryRepoAssociation> associations);

                            if (!TryAddEntities(new DeserializedInfo(toolName, addedDate, pub, associations)))
                                Logger.LogInformation($"Skipping tool {toolName}.");
                        }
                        else
                        {
                            Logger.LogDebug($"Cannot parse bibliography: {item.Value}");
                        }
                    }
                    catch (ArgumentException e)
                    {
                        Logger.LogError($"Error adding Bioconductor tool {item.Key}: {e.Message}");
                    }
            }

            File.Delete(citationsFileName);
        }

        private void GetDownloadStats()
        {
            Logger.LogDebug($"Downloading Bioconductor stats file {_statsFileName}.");

            var statsFileName = SessionTempPath + Utilities.GetRandomString();
            WebClient.DownloadFileTaskAsync(Repo.GetURI() + _statsFileName, statsFileName).Wait();

            string line;
            using var reader = new StreamReader(statsFileName);
            reader.ReadLine();
            while ((line = reader.ReadLine()) != null)
            {
                var cols = line.Split('\t');
                if (cols[2] == "all")
                    continue;

                if (!Tools.TryGetValue(cols[0].Trim(), out Tool tool))
                {
                    Logger.LogDebug($"Received stats for `{cols[0]}`, which is an unrecognized tool.");
                    continue;
                }

                try
                {
                    var date = new DateTime(
                       year: int.Parse(cols[1], CultureInfo.CurrentCulture),
                       month: DateTime.ParseExact(cols[2], "MMM", CultureInfo.CurrentCulture).Month,
                       day: 1);

                    ToolDownloadRecords.Add(new ToolDownloadRecord()
                    {
                        Tool = tool,
                        ToolID = tool.ID,
                        Date = date,
                        Count = int.Parse(cols[3], CultureInfo.CurrentCulture)
                    });
                }
                catch (Exception e)
                {
                    if (e is InvalidOperationException || e is FormatException)
                        Logger.LogDebug($"Error occurred reading stats file: {e.Message}");
                    continue;
                }
            }
        }

        private void GetAddedDate()
        {
            var dateAddedFileName = SessionTempPath + Utilities.GetRandomString();
            WebClient.DownloadFileTaskAsync(Repo.GetURI() + _dateAddedFileName, dateAddedFileName).Wait();
            _toolsAddToRepoDates = new Dictionary<string, DateTime?>();

            string line;
            using var reader = new StreamReader(dateAddedFileName);
            reader.ReadLine();
            while ((line = reader.ReadLine()) != null)
            {
                var cols = line.Split(',');
                _toolsAddToRepoDates.TryAdd(cols[1].Trim(),
                                            DateTime.Parse(cols[3], CultureInfo.CurrentCulture));
            }
        }
    }
}
