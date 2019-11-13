using Genometric.BibitemParser;
using Genometric.TVQ.API.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Web;

namespace Genometric.TVQ.API.Crawlers.Literature
{
    public class Scopus : BaseLiteratureCrawler
    {
        private static string BaseAPI
        {
            get { return "https://api.elsevier.com/content/"; }
        }

        private static string CitationsOverviewAPI
        {
            get { return BaseAPI + "abstract/citations/"; }
        }

        private static string APIKey
        {
            get { return Environment.GetEnvironmentVariable("SCOPUS_API_KEY"); }
        }

        private readonly List<Publication> _publications;
        private readonly ILogger<CrawlerService> _logger;

        public ConcurrentBag<Citation> Citations { get; }

        public Scopus(List<Publication> publications, ILogger<CrawlerService> logger)
        {
            _logger = logger;
            _publications = publications;
            Citations = new ConcurrentBag<Citation>();
        }

        public async Task CrawlAsync()
        {
            // This method is implemented using the Task Parallel Library (TPL).
            // Read the following page for more info on the flow: 
            // https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl

            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            var blockOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1
            };

            var updateWithScopusInfo = new TransformBlock<Publication, Publication>(
                new Func<Publication, Publication>(UpdateWithScopusInfo),
                blockOptions);

            var getCitations = new ActionBlock<Publication>(
                input => { GetCitations(input); },
                blockOptions);

            updateWithScopusInfo.LinkTo(getCitations, linkOptions);

            foreach (var publication in _publications)
                updateWithScopusInfo.Post(publication);

            updateWithScopusInfo.Complete();

            await getCitations.Completion.ConfigureAwait(false);
        }

        private Publication UpdateWithScopusInfo(Publication publication)
        {
            _logger.LogInformation($"Updating publication {publication.ID} info.");

            var uriBuilder = new UriBuilder("https://api.elsevier.com/content/search/scopus");
            var parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["apiKey"] = APIKey;
            if (TryGetQuery(publication, out string query))
                parameters["query"] = query;
            else
                return null;

            uriBuilder.Query = parameters.ToString();

            using var response = new HttpClient().GetAsync(uriBuilder.Uri).ConfigureAwait(false).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug($"Unsuccessful response for publication {publication.ID}: {response.StatusCode}; {response.ReasonPhrase}");
                return null;
            }

            var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            JToken obj = JObject.Parse(content);

            var entries = (JArray)obj["search-results"]["entry"];

            if (TryUpdatePublication(publication, entries))
                return publication;
            else
                return null;
        }

        private bool TryGetQuery(Publication publication, out string query)
        {
            query = null;
            if (publication.DOI != null)
            {
                query = $"DOI(\"{publication.DOI}\")";
            }
            else
            {
                switch (publication.Type)
                {
                    case BibTexEntryType.Article:
                    case BibTexEntryType.Book:
                    case BibTexEntryType.Inproceedings:
                    case BibTexEntryType.Misc:
                        ///var title = new Regex(@".*title={(?<title>.+)}.*").Match(publication.BibTeXEntry).Groups["title"].Value;
                        //var author = new Regex(@".*author={(?<author>.+)}.*").Match(publication.Citation).Groups["author"].Value;
                        //var year = new Regex(@".*year={(?<year>.+)}.*").Match(publication.Citation).Groups["year"].Value;

                        query = $"TITLE(\"{publication.Title}\")";
                        break;

                    default:
                        _logger.LogDebug($"Skipping publication {publication.ID} because of unsupported type {publication.Type}.");
                        return false;
                }
            }

            return true;
        }

        private bool TryUpdatePublication(Publication publication, JArray response)
        {
            if (response.Any(x => ((JObject)x).ContainsKey("error")))
            {
                _logger.LogDebug($"Skipping publication {publication.ID} for the following error: {response}");
                return false;
            }

            if (response.Count == 0)
            {
                _logger.LogDebug($"Skipping publication {publication.ID} because no record found on Scopus.");
                return false;
            }

            if (response.Count > 1)
            {
                _logger.LogDebug($"Skipping publication {publication.ID} because more than one record found on Scopus.");
                return false;
            }

            publication.EID = ExtractFromResponse(response, "eid");
            publication.ScopusID = ExtractFromResponse(response, "dc:identifier").Split(':')[1];

            if (publication.Title == null)
                publication.Title = ExtractFromResponse(response, "dc:title");

            if (publication.Volume == null)
                publication.Volume = Convert.ToInt32(ExtractFromResponse(response, "prism:volume"), CultureInfo.InvariantCulture);

            if (publication.Pages == null)
                publication.Pages = ExtractFromResponse(response, "prism:pageRange");

            if (publication.Journal == null)
                publication.Journal = ExtractFromResponse(response, "prism:publicationName");

            if (publication.Year == null)
            {
                var date = DateTime.Parse(ExtractFromResponse(response, "prism:coverDate"), CultureInfo.InvariantCulture);
                publication.Year = date.Year;
                publication.Month = date.Month;
            }

            return true;
        }

        private static string ExtractFromResponse(JArray response, string field)
        {
            return response.Select(x => x[field]).First().ToString();
        }

        private void GetCitations(Publication publication)
        {
            if (publication == null || publication.Year == null)
            {
                _logger.LogDebug($"Skipping publication {publication.ID} because cannot determine publication year.");
                return;
            }

            DateTime startDate = new DateTime((int)publication.Year, publication.Month == null ? 01 : (int)publication.Month, 01);
            DateTime endDate = DateTime.Now;

            // The currently supported view by Scopus.
            string view = "STANDARD";
            string date = $"{startDate.Year}-{endDate.Year}";
            var uriBuilder = new UriBuilder(CitationsOverviewAPI + publication.EID);
            var parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["apiKey"] = APIKey;
            parameters["view"] = view;
            parameters["date"] = date;
            parameters["scopus_id"] = publication.ScopusID;

            uriBuilder.Query = parameters.ToString();

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("X-ELS-APIKey", "APIKey");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "TVQv1");
                HttpResponseMessage response = client.GetAsync(uriBuilder.Uri).ConfigureAwait(false).GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogDebug($"Skipping publication {publication.ID} for the following error getting citation information: {response.StatusCode}; {response.ReasonPhrase}");
                    return;
                }

                var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                TryUpdatePublicationCitation(publication, JObject.Parse(content));
            }
            catch (Exception e)
            {
                _logger.LogDebug($"Skipping publication {publication.ID} due to the following error: {e.Message}");
            }
        }

        private bool TryUpdatePublicationCitation(Publication publication, JObject response)
        {
            var citeCountHeader = response["abstract-citations-response"]["citeColumnTotalXML"]["citeCountHeader"];
            if (citeCountHeader["columnHeading"] is JArray)
            {
                var years = ((JArray)citeCountHeader["columnHeading"]).Select(c => (int)c["$"]).ToList();
                var citationCount = ((JArray)citeCountHeader["columnTotal"]).Select(c => (int)c["$"]).ToList();

                for (int i = 0; i < years.Count; i++)
                    AddCitation(publication, new DateTime(years[i], 01, 01), citationCount[i]);
            }
            else
            {
                var years = (int)(JValue)citeCountHeader["columnHeading"];
                var citationCount = (int)(JValue)citeCountHeader["columnTotal"];
                AddCitation(publication, new DateTime(years, 01, 01), citationCount);
            }

            return true;
        }

        private void AddCitation(Publication publication, DateTime date, int count)
        {
            Citations.Add(new Citation()
            {
                Date = date,
                Count = count,
                Source = Citation.InfoSource.Scopus,
                Publication = publication
            });
        }
    }
}
