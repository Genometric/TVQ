using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    public class LiteratureCrawlingJob : BaseJob
    {
        public List<Publication> Publications { set; get; }
    }
}
