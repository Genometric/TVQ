using Genometric.BibitemParser;
using Genometric.TVQ.API.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
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

        public Scopus(List<Publication> publications, ILogger<CrawlerService> logger)
        {
            _logger = logger;
            _publications = publications;
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

            HttpResponseMessage response = null;
            try
            {
                using var client = new HttpClient();
                response = client.GetAsync(uriBuilder.Uri).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (HttpRequestException e)
            {
                // TODO: log this exception.
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                LogSkippedPublications(publication, $" {response.StatusCode}; {response.ReasonPhrase}; {response.Headers}");
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
            else if (publication.PubMedID != null)
            {
                query = $"PMID(\"{publication.PubMedID}\")";
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

                        if (string.IsNullOrWhiteSpace(publication.Title))
                        {
                            LogSkippedPublications(publication, "missing title");
                            return false;
                        }
                        query = $"TITLE(\"{publication.Title}\")";
                        break;

                    default:
                        LogSkippedPublications(publication, $"unsupported type {publication.Type}");
                        return false;
                }
            }

            return true;
        }

        private bool TryUpdatePublication(Publication publication, JArray response)
        {
            if (response.Any(x => ((JObject)x).ContainsKey("error")))
            {
                LogSkippedPublications(
                    publication,
                    response
                    .First(x => ((JObject)x).ContainsKey("error"))
                    .Last().Value<JProperty>().Value.ToString());
                return false;
            }

            if (response.Count == 0)
            {
                LogSkippedPublications(publication, "no record found");
                return false;
            }

            if (response.Count > 1)
            {
                LogSkippedPublications(publication, "more than one record found");
                return false;
            }

            if (!TryExtractFromResponse(response, "eid", out string eid) ||
                !TryExtractFromResponse(response, "dc:identifier", out string id))
            {
                LogSkippedPublications(publication, "cannot determine Scopus ID or EID");
                return false;
            }
            else
            {
                publication.EID = eid;
                publication.ScopusID = id.Split(':')[1];
            }

            if (publication.DOI == null &&
                TryExtractFromResponse(response, "prism:doi", out string doi))
                publication.DOI = doi;

            if (publication.Title == null &&
                TryExtractFromResponse(response, "dc:title", out string title))
                publication.Title = title;

            if (publication.Volume == null &&
                TryExtractFromResponse(response, "prism:volume", out string volume))
                publication.Volume = volume;

            if (publication.Pages == null &&
                TryExtractFromResponse(response, "prism:pageRange", out string pageRange))
                publication.Pages = pageRange;

            if (publication.Journal == null &&
                TryExtractFromResponse(response, "prism:publicationName", out string publicationName))
                publication.Journal = publicationName;

            if (publication.Year == null &&
                TryExtractFromResponse(response, "prism:coverDate", out string coverDate))
            {
                var date = DateTime.Parse(coverDate, CultureInfo.InvariantCulture);
                publication.Year = date.Year;
                publication.Month = date.Month;
                publication.Day = date.Day;
            }

            if (publication.PubMedID == null &&
                TryExtractFromResponse(response, "pubmed-id", out string pmid))
                publication.PubMedID = pmid;

            if (TryExtractFromResponse(response, "citedby-count", out string citedBy) &&
                int.TryParse(citedBy, out int c))
                publication.CitedBy = c;

            return true;
        }

        private static bool TryExtractFromResponse(JArray response, string field, out string value)
        {
            value = null;
            var v = response.Select(x => x[field]);
            if (v == null || v.First() == null)
                return false;
            value = v.First().ToString();
            return true;
        }

        private void GetCitations(Publication publication)
        {
            if (publication == null)
            {
                return;
            }
            if (publication.Year == null)
            {
                LogSkippedPublications(publication, "cannot determine publication year");
                return;
            }

            DateTime startDate = new DateTime(
                (int)publication.Year,
                publication.Month == null ? 01 : (int)publication.Month,
                publication.Day == null ? 01 : (int)publication.Day);

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
                    LogSkippedPublications(publication, $"{response.StatusCode}; {response.ReasonPhrase}");
                    return;
                }

                var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                TryUpdatePublicationCitation(publication, JObject.Parse(content));
            }
            catch (Exception e)
            {
                LogSkippedPublications(publication, e.Message);
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

        private static void AddCitation(Publication publication, DateTime date, int count)
        {
            if (publication.Citations == null)
                publication.Citations = new List<Citation>();

            publication.Citations.Add(new Citation()
            {
                Date = date,
                Count = count,
                Source = Citation.InfoSource.Scopus,
                Publication = publication
            });
        }

        private void LogSkippedPublications(Publication publication, string reason)
        {
            _logger.LogDebug($"Skipping publication {publication.ID}: {reason}");
        }
    }
}
