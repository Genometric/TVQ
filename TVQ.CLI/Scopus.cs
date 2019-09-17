using Genometric.TVQ.API.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Genometric.TVQ.CLI
{
    public class Scopus
    {
        private string _apiKey;

        public Scopus(string apiKey)
        {
            _apiKey = apiKey;
        }

        public void GetCitations(string toolsPath, string totalCitationsFileName)
        {
            int counter = 0;
            Console.WriteLine("Getting citations from Scopus ...");
            using (StreamWriter writer = new StreamWriter(totalCitationsFileName))
            {
                var files = Directory.GetFiles(toolsPath);
                foreach (var file in files)
                {
                    Console.Write(string.Format("Tool {0}/{1} ...", ++counter, files.Length));
                    using (var reader = new StreamReader(file))
                    {
                        var jReader = new JsonTextReader(reader);
                        var tool = new JsonSerializer().Deserialize<ExtTool>(jReader);
                        var totalCitations = GetCitationCount(tool).Result;
                        writer.WriteLine(
                            string.Format(
                                "{0}\t{1}\t{2}",
                                tool.IDinRepo,
                                tool.Name,
                                totalCitations));
                    }

                    Console.WriteLine("\tDone!");
                }
            }
        }

        private async Task<int> GetCitationCount(ExtTool tool)
        {
            int totalCitations = 0;
            foreach (var pub in tool.Publications)
                totalCitations += await FetchCitation(pub);
            return totalCitations;
        }

        private async Task<int> FetchCitation(Publication publication)
        {
            var _client = new HttpClient();

            var uriBuilder = new UriBuilder("https://api.elsevier.com/content/search/scopus");
            var parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["apiKey"] = _apiKey;
            if (publication.DOI != null)
            {
                parameters["query"] = string.Format("DOI(\"{0}\")", publication.DOI);
            }
            else
            {
                var type = new Regex(@".*@(?<type>.+){.*").Match(publication.Citation).Groups["type"].Value.ToLower().Trim();
                switch(type)
                {
                    case "misc":
                    case "article":
                    case "book":
                    case "inproceedings":
                        var title = new Regex(@".*title={(?<title>.+)}.*").Match(publication.Citation).Groups["title"].Value;
                        //var author = new Regex(@".*author={(?<author>.+)}.*").Match(publication.Citation).Groups["author"].Value;
                        //var year = new Regex(@".*year={(?<year>.+)}.*").Match(publication.Citation).Groups["year"].Value;
                        parameters["query"] = string.Format("TITLE(\"{0}\")", title);
                        break;

                    default:
                        // not supported type at the moment.
                        break;

                }
            }
            uriBuilder.Query = parameters.ToString();

            HttpResponseMessage response = await _client.GetAsync(uriBuilder.Uri);
            if (!response.IsSuccessStatusCode)
                /// TODO: replace with an exception.
                return 0;

            var content = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(content);

            try
            {
                int totalResults = (int)obj["search-results"]["opensearch:totalResults"];
                switch(totalResults)
                {
                    case 0:
                        Console.Write("No results found.");
                        return 0;

                    case 1:
                        break;

                    default:
                        Console.Write("More than one result found, summing their citation count.");
                        break;
                }
                var citations = from entry in obj["search-results"]["entry"]
                                select (int)entry["citedby-count"];
                return citations.Sum();
            }
            catch (Exception e)
            {
                Console.Write("Error: " + e.Message);
            }

            return 0;
        }
    }
}
