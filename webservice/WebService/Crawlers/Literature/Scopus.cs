using Genometric.BibitemParser;
using Genometric.TVQ.WebService.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.WebService.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Web;

namespace Genometric.TVQ.WebService.Crawlers.Literature
{
    public class Scopus : BaseLiteratureCrawler
    {
        private int _publicationToScan;

        private int _scannedPublications;

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

        public Scopus(List<Publication> publications,
                      ILogger<BaseService<LiteratureCrawlingJob>> logger) :
            base(publications, logger)
        { }

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

            var mergePubsIfNecessary = new TransformBlock<Publication, Publication>(
                new Func<Publication, Publication>(MergePubsIfNecessary),
                blockOptions);

            var getCitations = new ActionBlock<Publication>(
                input => { GetCitations(input); },
                blockOptions);

            updateWithScopusInfo.LinkTo(mergePubsIfNecessary, linkOptions);
            mergePubsIfNecessary.LinkTo(getCitations, linkOptions);

            _publicationToScan = Publications.Count;
            _scannedPublications = 0;
            foreach (var publication in Publications)
                updateWithScopusInfo.Post(publication);

            updateWithScopusInfo.Complete();

            await getCitations.Completion.ConfigureAwait(false);
        }

        private Publication UpdateWithScopusInfo(Publication publication)
        {
            Interlocked.Increment(ref _scannedPublications);
            Logger.LogInformation($"Updating publication {_scannedPublications}/{_publicationToScan}: {publication.ID}");

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
                Logger.LogDebug($"Querying Scopus using `{query}`.");
                response = client.GetAsync(uriBuilder.Uri).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (HttpRequestException e)
            {
                Logger.LogError($"Exception querying Scopus: {e.Message}. Check your Internet connection.");
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
            {
                TryRemovePublication(publication);
                return null;
            }
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
                // It is better to keep a title-based search as last option,
                // because Scopus's title-based search is exact-match-based; 
                // i.e., if the title is missing a word or a word is misspelled
                // then Scopus will not find a match. 
                if (string.IsNullOrWhiteSpace(publication.Title))
                {
                    LogSkippedPublications(publication, "missing title");
                    return false;
                }

                query = $"TITLE({FormatPublicationTitle(publication.Title)})";
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
                // When multiple publications are found,
                // then choose the most current one.

                static DateTime GetDate(JToken input)
                {
                    if (TryExtractFromResponse(input, "prism:coverDate", out string date))
                        return DateTime.Parse(date, CultureInfo.InvariantCulture);
                    else
                        return DateTime.Today;
                }

                static bool AssertErrata(JToken input)
                {
                    if (TryExtractFromResponse(input, "subtypeDescription", out string type))
                        if (string.Equals(type, "erratum", StringComparison.InvariantCultureIgnoreCase))
                            return true;

                    if (TryExtractFromResponse(input, "dc:title", out string title))
                        return title.StartsWith("corrigendum:", StringComparison.InvariantCultureIgnoreCase);

                    return false;
                }

                var selectedToken = response.Aggregate(
                    // Seed value.
                    response[0],

                    // Condition: choose the latest publication which is not a correction 
                    // (determined using the paper title: a correction paper usually has 
                    // `corrigendum:` in its title).
                    (mostCurrent, next) =>
                    {
                        return (
                        // The selected publication should not be a correction.
                        AssertErrata(mostCurrent)

                        // The next publication should not be a correction, 
                        // and more current than the selected publication.
                        || (!AssertErrata(next) && GetDate(next) > GetDate(mostCurrent)))

                        // If above condition are met, the replace the 
                        // currently selected publication with the next 
                        // publication in the JArray.
                        ? next : mostCurrent;
                    },

                    // Return the currently selected publication.
                    x => x);

                Logger.LogDebug($"Found {response.Count} publications, chose the most current one.");
                response = new JArray(selectedToken);
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

            if (TryExtractFromResponse(response, "prism:doi", out string doi))
                publication.DOI = doi;

            if (TryExtractFromResponse(response, "dc:title", out string title))
                publication.Title = title;

            if (TryExtractFromResponse(response, "prism:volume", out string volume))
                publication.Volume = volume;

            if (TryExtractFromResponse(response, "prism:pageRange", out string pageRange))
                publication.Pages = pageRange;

            if (TryExtractFromResponse(response, "prism:publicationName", out string publicationName))
                publication.Journal = publicationName;

            if (TryExtractFromResponse(response, "prism:coverDate", out string coverDate))
            {
                var date = DateTime.Parse(coverDate, CultureInfo.InvariantCulture);
                publication.Year = date.Year;
                publication.Month = date.Month;
                publication.Day = date.Day;
            }

            if (TryExtractFromResponse(response, "pubmed-id", out string pmid))
                publication.PubMedID = pmid;

            if (TryExtractFromResponse(response, "citedby-count", out string citedBy) &&
                int.TryParse(citedBy, out int c))
                publication.CitedBy = c;

            // A publication's type may be set to types 
            // such as InPrepration, or Manual, and not 
            // updated once a related publication is 
            // published. This section can get the type 
            // of such publications updated. 
            if (TryExtractFromResponse(response, "subtypeDescription", out string type))
                if (Enum.TryParse(type, ignoreCase: true, out BibTexEntryType bibTexEntryType))
                    publication.Type = bibTexEntryType;

            return true;
        }

        private static bool TryExtractFromResponse(JToken response, string field, out string value)
        {
            value = null;
            if (response == null)
                return false;
            value = response[field].ToString();
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

            ComputeAccumulatedCitationCounts(publication);

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

        private static void ComputeAccumulatedCitationCounts(Publication publication)
        {
            var accumulatedCitationsCount = new SortedList<DateTime, int>();
            foreach (var citation in publication.Citations)
                accumulatedCitationsCount.Add(citation.Date, citation.Count);

            var dates = accumulatedCitationsCount.Keys;
            for (int i = 1; i < dates.Count; i++)
                accumulatedCitationsCount[dates[i]] += accumulatedCitationsCount[dates[i - 1]];

            foreach (var citation in publication.Citations)
                citation.AccumulatedCount = accumulatedCitationsCount[citation.Date];
        }

        private void LogSkippedPublications(Publication publication, string reason)
        {
            Logger.LogDebug($"Skipping publication {publication.ID}: {reason}");
        }
    }
}
