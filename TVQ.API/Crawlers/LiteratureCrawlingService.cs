using Genometric.TVQ.API.Crawlers.Literature;
using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Crawlers
{
    public class LiteratureCrawlingService : BaseService<LiteratureCrawlingJob>
    {
        public LiteratureCrawlingService(
            TVQContext context,
            ILogger<LiteratureCrawlingService> logger) :
            base(context, logger)
        { }

        protected override async Task ExecuteAsync(
            LiteratureCrawlingJob job,
            CancellationToken cancellationToken)
        {
            if (job.ScanAllPublications)
                job.Publications = Context.Publications.ToList();

            Context.Entry(job).Collection(x => x.Publications).Load();
            using var scopusCrawler = new Scopus(job.Publications, Logger);
            await scopusCrawler.CrawlAsync().ConfigureAwait(false);
            //Map();

            /// It is better to let the scheduler to set the status and 
            /// save the changes. However, since the scheduler is in a 
            /// different database context, it wont see the changes 
            /// in this context, hence cannot save the citations. 
            /// Also, if we let citations to be saved in this context
            /// and job status set in the scheduler, in that case, 
            /// if any error occurs in scheduler before setting/saving
            /// job status, then it will corrupt database (citations 
            /// saved, but job either erred or pending; in violation of 
            /// ACID).
            job.Status = State.Completed;
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }


        // THIS IS AN EXPERIMENTAL METHOD.
        private void Map()
        {
            var tools = Context.Tools.Include(x => x.PublicationAssociations)
                .ThenInclude(x => x.Publication.Citations)
                .Include(x => x.RepoAssociations)
                .ThenInclude(x => x.Repository)
                .ToList();

            var toolsDict = new Dictionary<string, Tool>();

            foreach (var tool in tools)
            {
                foreach (var association in tool.PublicationAssociations)
                {
                    if (association.Publication.DOI != null)
                    {
                        if (!toolsDict.ContainsKey(association.Publication.DOI))
                        {
                            toolsDict.Add(association.Publication.DOI, tool);
                        }
                        else
                        {
                            var merged = Merge(toolsDict[association.Publication.DOI], tool);
                            Context.Tools.Remove(tool);
                            Context.Tools.Remove(toolsDict[association.Publication.DOI]);
                            Context.Tools.Add(merged);
                            //toolsDict.Add(publication.DOI, merged);
                        }
                    }
                }
            }
        }

        private Tool Merge(Tool a, Tool b)
        {
            return a;
            foreach (PropertyInfo prop in typeof(Tool).GetProperties())
                if (prop.GetValue(a) == null && prop.GetValue(b) != null)
                    prop.SetValue(a, prop.GetValue(b));

            return a;
        }
    }
}
