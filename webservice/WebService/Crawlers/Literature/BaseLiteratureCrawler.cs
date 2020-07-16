using Genometric.TVQ.WebService.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.WebService.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Genometric.TVQ.WebService.Crawlers.Literature
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
