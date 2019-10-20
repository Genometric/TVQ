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

        public Bioconductor(Repository repo, List<Tool> tools) : base(repo, tools)
        { }

        public override async Task ScanAsync()
        {
            ReadCitationsFile();
            GetDownloadStats();
        }

        private void ReadCitationsFile()
        {
            var citationsFileName = SessionTempPath + Utilities.GetRandomString();
            WebClient.DownloadFileTaskAsync(Repo.GetURI() + _citationsFileName, citationsFileName).Wait();

            using (var reader = new StreamReader(citationsFileName))
            {
                string json = reader.ReadToEnd();
                var items = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                foreach (var item in items)
                    TryAddEntities(
                        new Tool() { Name = item.Key.Trim() },
                        new Publication() { BibTeXEntry = item.Value });
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
    }
}
