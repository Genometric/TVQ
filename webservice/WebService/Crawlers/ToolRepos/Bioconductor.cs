﻿using Genometric.TVQ.WebService.Crawlers.ToolRepos.HelperTypes;
using Genometric.TVQ.WebService.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.WebService.Model;
using Genometric.TVQ.WebService.Model.Associations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Genometric.TVQ.WebService.Crawlers.ToolRepos
{
    public class Bioconductor : BaseToolRepoCrawler
    {
        private readonly string _citationsFileName = "citations.json";
        private readonly string _statsFileName = "package_stats.tsv";
        private readonly string _dateAddedFileName = "first_appearance.json";
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

            try
            {
                WebClient.DownloadFileTaskAsync(Repo.GetURI() + _dateAddedFileName, dateAddedFileName).Wait();
            }
            catch (AggregateException e)
            {
                Logger.LogError(
                    $"Cannot download Bioconductor file {Repo.GetURI() + _dateAddedFileName}; " +
                    $"Container may not be able to access the Internet; Error: {e.Message}");
            }


            var datesJson = File.ReadAllText(dateAddedFileName);
            var dates = JsonSerializer.Deserialize<Dictionary<string, BioconductorRelease>>(datesJson);

            _toolsAddToRepoDates = new Dictionary<string, DateTime?>();
            foreach (var version in dates)
                foreach (var package in version.Value.Packages)
                    _toolsAddToRepoDates.TryAdd(
                        package,
                        DateTime.Parse(version.Value.ReleaseDate, CultureInfo.CurrentCulture));
        }
    }
}
