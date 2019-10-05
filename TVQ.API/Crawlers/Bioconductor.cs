using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Model;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Crawlers
{
    public class Bioconductor : ToolRepoCrawler
    {
        private readonly string _citationsFileName = "citations.json";

        public Bioconductor(TVQContext dbContext, Repository repo) :
            base(dbContext, repo)
        { }

        public override async Task ScanAsync()
        {
            ReadCitationsFile();
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
                    var tool = new Tool() { Name = item.Key, Repository = _repo };
                    var pubs = new List<Publication> { new Publication() { BibTeXEntry = item.Value, Tool = tool } };
                    AddEntities(tool, pubs);
                }
            }
        }
    }
}
