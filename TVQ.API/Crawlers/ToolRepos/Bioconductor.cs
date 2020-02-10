using Genometric.TVQ.API.Model;
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

        public Bioconductor(
            Repository repo,
            List<Tool> tools,
            List<Publication> publications,
            List<Category> categories) :
            base(repo, tools, publications, categories)
        {
            BibitemParser.KeywordsDelimiter = ',';
        }

        public override async Task ScanAsync()
        {
            ReadCitationsFile();
            GetDownloadStats();
            GetAddedDate();
        }

        private void ReadCitationsFile()
        {
            var citationsFileName = SessionTempPath + Utilities.GetRandomString();

            /// Use a new WebClient instance for downloads, because 
            /// an instance of WebClient does not support concurrent
            /// downloads. 
            new WebClient().DownloadFileTaskAsync(Repo.GetURI() + _citationsFileName, citationsFileName).Wait();

            using (var reader = new StreamReader(citationsFileName))
            {
                string json = reader.ReadToEnd();
                var items = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                foreach (var item in items)
                    try
                    {
                        if (TryParseBibitem(item.Value, out Publication pub))
                            TryAddEntities(
                                new Tool() { Name = item.Key.Trim() },
                                pub);
                    }
                    catch (ArgumentException e)
                    {

                    }
            }

            File.Delete(citationsFileName);
        }

        private void GetDownloadStats()
        {
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

                if (!ToolsDict.TryGetValue(cols[0], out Tool tool))
                {
                    // TODO: log this.
                    // This mean the tool for which we have stats 
                    // is not recognized.
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
                catch (InvalidOperationException e)
                {
                    // TODO: log exception and continue, do NOT break the while loop.
                }
                catch (FormatException e)
                {
                    // TODO: log exception and continue, do NOT break the while loop.
                }
                catch (Exception e)
                {
                    // TODO: log exception and continue, do NOT break the while loop.
                }
            }
        }

        private void GetAddedDate()
        {
            var dateAddedFileName = SessionTempPath + Utilities.GetRandomString();
            WebClient.DownloadFileTaskAsync(Repo.GetURI() + _dateAddedFileName, dateAddedFileName).Wait();

            string line;
            using var reader = new StreamReader(dateAddedFileName);
            reader.ReadLine();
            while ((line = reader.ReadLine()) != null)
            {
                var cols = line.Split(',');
                if (!ToolsDict.TryGetValue(FormatToolName(cols[1]), out Tool tool))
                {
                    // TODO: log this.
                    // This mean the tool for which we have added date 
                    // is not recognized.
                    continue;
                }

                try
                {
                    UpdateAssociation(tool, DateTime.Parse(cols[3], CultureInfo.CurrentCulture));
                }
                catch (Exception e)
                {
                    // TODO: log exception and continue, do NOT break the while loop.
                }
            }
        }
    }
}
