using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Crawlers
{
    public class Bioconductor : ToolRepoCrawler
    {
        private readonly string _citationsFileName = "citations.json";
        private readonly string _statsFileName = "package_stats.tsv";

        public Bioconductor(TVQContext dbContext, Repository repo) :
            base(dbContext, repo)
        { }

        public override async Task ScanAsync()
        {
            ReadCitationsFile();
            ReadDownloadStats();
        }

        private void ReadCitationsFile()
        {
            var citationsFileName = _sessionTempPath + Utilities.GetRandomString();
            _webClient.DownloadFileTaskAsync(_repo.GetURI() + _citationsFileName, citationsFileName).Wait();

            using (var reader = new StreamReader(citationsFileName))
            {
                string json = reader.ReadToEnd();
                var items = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                foreach (var item in items)
                {
                    if (Tools.ContainsKey(item.Key.Trim()))
                    {
                        // TODO: log this.
                        continue;
                    }

                    TryAddEntities(
                        new Tool() { Name = item.Key.Trim() },
                        new Publication() { BibTeXEntry = item.Value });
                }
            }

            _dbContext.SaveChanges();
            File.Delete(citationsFileName);
        }

        private void ReadDownloadStats()
        {
            var statsFileName = _sessionTempPath + Utilities.GetRandomString();
            _webClient.DownloadFileTaskAsync(_repo.GetURI() + _statsFileName, statsFileName).Wait();

            string line;
            using var reader = new StreamReader(statsFileName);
            reader.ReadLine();
            while ((line = reader.ReadLine()) != null)
            {
                var cols = line.Split('\t');
                if (cols[2] == "all")
                    continue;

                if (!Tools.TryGetValue(cols[0], out Tool tool))
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

                    var downloadRecord = new ToolDownloadRecord()
                    {
                        Tool = tool,
                        ToolID = tool.ID,
                        Date = date,
                        Count = int.Parse(cols[3], CultureInfo.CurrentCulture)
                    };

                    _dbContext.ToolDownloadRecords.Add(downloadRecord);
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
    }
}
