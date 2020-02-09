using Genometric.TVQ.API.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.API.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Crawlers.Literature
{
    public class BaseLiteratureCrawler : BaseCrawler
    {
        protected ILogger<BaseService<LiteratureCrawlingJob>> Logger { set; get; }

        public BaseLiteratureCrawler(List<Publication> publications,
                                     ILogger<BaseService<LiteratureCrawlingJob>> logger) :
            base(publications)
        {
            Logger = logger;
        }
    }
}
