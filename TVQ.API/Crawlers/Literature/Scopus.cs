using Genometric.TVQ.API.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
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

        private List<Publication> _publications;

        public ConcurrentBag<Citation> Citations { get; }

        public Scopus(List<Publication> publications)
        {
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
            var uriBuilder = new UriBuilder("https://api.elsevier.com/content/search/scopus");
            var parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["apiKey"] = APIKey;
            if (publication.DOI != null)
            {
                parameters["query"] = $"DOI(\"{publication.DOI}\")";
            }
            else
            {
                var type = new Regex(@".*@(?<type>.+){.*").Match(publication.BibTeXEntry).Groups["type"].Value.ToLower().Trim();
                switch (type)
                {
                    case "misc":
                    case "article":
                    case "book":
                    case "inproceedings":
                        var title = new Regex(@".*title={(?<title>.+)}.*").Match(publication.BibTeXEntry).Groups["title"].Value;
                        //var author = new Regex(@".*author={(?<author>.+)}.*").Match(publication.Citation).Groups["author"].Value;
                        //var year = new Regex(@".*year={(?<year>.+)}.*").Match(publication.Citation).Groups["year"].Value;
                        parameters["query"] = string.Format("TITLE(\"{0}\")", title);
                        break;

                    default:
                        // not supported type at the moment.
                        return null;
                }
            }

            uriBuilder.Query = parameters.ToString();

            HttpResponseMessage response = HttpClient.GetAsync(uriBuilder.Uri).ConfigureAwait(false).GetAwaiter().GetResult(); //await _client.GetAsync(uriBuilder.Uri).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                /// TODO: replace with an exception.
                return null;
            }

            var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult(); //await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var obj = JObject.Parse(content);

            var entries = (JArray)obj["search-results"]["entry"];

            if (entries.Count == 0)
            {
                // log error
                return null;
            }
            else if (entries.Count >= 1)
            {
                // log error
                return null;
            }

            publication.EID = entries.Select(x => x["eid"]).First().ToString();
            publication.ScopusID = entries.Select(x => x["dc:identifier"]).First().ToString().Split(':')[1];
            return publication;
        }

        private void GetCitations(Publication publication)
        {
            if (publication == null)
                return;

            DateTime startDate = new DateTime(Convert.ToInt32(publication.Year), 01, 01);
            DateTime endDate = DateTime.Now;

            // The currently supported view by Scopus.
            string view = "STANDARD";
            string date = $"{startDate.Year}-{endDate.Year}";
            var uriBuilder = new UriBuilder(CitationsOverviewAPI + publication.EID); //"2-s2.0-84860718683") ;
            var parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["apiKey"] = APIKey;
            parameters["view"] = view;
            parameters["date"] = date;

            /// EDI is like this: 2-s2.0-84860718683 and scopus ID is this part of it: 84860718683
            parameters["scopus_id"] = publication.ScopusID; // "84860718683";

            uriBuilder.Query = parameters.ToString();
            HttpClient.DefaultRequestHeaders.Add("X-ELS-APIKey", "APIKey");
            HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            HttpClient.DefaultRequestHeaders.Add("User-Agent", "TVQv1");

            try
            {
                var tmpuri = uriBuilder.Uri;
                HttpResponseMessage response = HttpClient.GetAsync(uriBuilder.Uri).ConfigureAwait(false).GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                {
                    /// TODO: replace with an exception.
                    //return 0;
                }

                var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                var obj = JObject.Parse(content);
                var columnHeading = (JArray)obj["abstract-citations-response"]["citeColumnTotalXML"]["citeCountHeader"]["columnHeading"];
                var years = columnHeading.Select(c => (int)c["$"]).ToList();
                var columnTotal = (JArray)obj["abstract-citations-response"]["citeColumnTotalXML"]["citeCountHeader"]["columnTotal"];
                var citationCount = columnTotal.Select(c => (int)c["$"]).ToList();

                for (int i = 0; i < years.Count; i++)
                {
                    Citations.Add(new Citation()
                    {
                        PublicationID = 0,
                        Date = new DateTime(years[i], 01, 01),
                        Count = citationCount[i],
                        Source = Citation.InfoSource.Scopus
                    });
                }
            }
            catch (Exception e)
            {

            }

            throw new Exception();
        }
    }
}
