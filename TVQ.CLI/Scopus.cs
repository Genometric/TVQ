using Genometric.TVQ.API.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        private readonly string _apiKey;

        private string BaseAPI { get { return "https://api.elsevier.com/content/"; } }
        private string CitationOverviewAPI { get { return BaseAPI + "abstract/citations/"; } }

        private List<string> _moreThanOneCitations;
        public ReadOnlyCollection<string> MoreThanOneCitations
        {
            get
            {
                return _moreThanOneCitations.AsReadOnly();
            }
        }

        private List<string> _zeroCitations;
        public ReadOnlyCollection<string> ZeroCitations
        {
            get
            {
                return _zeroCitations.AsReadOnly();
            }
        }

        public Scopus(string apiKey)
        {
            _apiKey = apiKey;
            _moreThanOneCitations = new List<string>();
            _zeroCitations = new List<string>();
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
                    Console.Write(string.Format("\r\tProcessing tool {0}/{1}", ++counter, files.Length));
                    using (var reader = new StreamReader(file))
                    {
                        var jReader = new JsonTextReader(reader);
                        var tool = new JsonSerializer().Deserialize<ExtTool>(jReader);
                        var totalCitations = GetCitationCount(tool).Result;
                        var aaaa = GetCitationCount(tool, new DateTime(2016, 01, 01), new DateTime(2019, 01, 01));
                        writer.WriteLine(
                            string.Format(
                                "{0}\t{1}\t{2}",
                                tool.IDinRepo,
                                totalCitations,
                                tool.Name));
                    }
                }
            }

            if (_moreThanOneCitations.Count > 0)
            {
                var fileName = AppDomain.CurrentDomain.BaseDirectory + "MoreThanOneCitation.txt";
                using (var writer = new StreamWriter(fileName))
                {
                    writer.WriteLine("Tool_id\tSearch_result_count");
                    foreach (var item in _moreThanOneCitations)
                        writer.WriteLine(item);
                }
                Console.WriteLine(string.Format(
                    "\n\n\tAggregated multiple citations for {0} tools." +
                    " Details logged in file {1}.", _moreThanOneCitations.Count, fileName));
            }

            if (_zeroCitations.Count > 0)
            {
                var fileName = AppDomain.CurrentDomain.BaseDirectory + "ZeroCitations.txt";
                using (var writer = new StreamWriter(fileName))
                    foreach (var item in _zeroCitations)
                        writer.WriteLine(item);
                Console.WriteLine(string.Format(
                    "\tSkipped {0} tools with zero citations. " +
                    "Details logged in file {1}.\n\n", _zeroCitations.Count, fileName));
            }
        }

        private async Task<int> GetCitationCount(ExtTool tool)
        {
            int totalCitations = 0;
            foreach (var pub in tool.Publications)
                totalCitations += await FetchCitation(tool, pub);
            return totalCitations;
        }

        public async Task<int> GetCitationCount(ExtTool tool, DateTime startDate, DateTime endDate)
        {
            // The currently supported view by Scopus.
            string view = "STANDARD";
            string date = string.Format("{0}-{1}", startDate.Year, endDate.Year);
            var uriBuilder = new UriBuilder(CitationOverviewAPI + "2-s2.0-84860718683");
            var parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["apiKey"] = _apiKey;
            parameters["view"] = view;
            parameters["date"] = date;

            /// EDI is like this: 2-s2.0-84860718683 and scopus ID is this part of it: 84860718683
            parameters["scopus_id"] = "84860718683";

            uriBuilder.Query = parameters.ToString();
            var _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("X-ELS-APIKey", "APIKey");
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("User-Agent", "TVQv1");

            try
            {
                var tmpuri = uriBuilder.Uri;
                HttpResponseMessage response = await _client.GetAsync(uriBuilder.Uri);
                if (!response.IsSuccessStatusCode)
                    /// TODO: replace with an exception.
                    return 0;

                var content = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(content);
            }
            catch (Exception e)
            {

            }

            return 0;
        }

        private async Task<int> FetchCitation(ExtTool tool, Publication publication)
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
                        return 0;

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
                        _zeroCitations.Add(tool.IDinRepo);
                        return 0;

                    case 1:
                        break;

                    default:
                        _moreThanOneCitations.Add(string.Format("{0}\t{1}", tool.IDinRepo, totalResults));
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
