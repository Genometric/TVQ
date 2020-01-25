using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    public class LiteratureCrawlingJob : BaseJob
    {
        public bool ScanAllPublications { set; get; } = false;
        public List<Publication> Publications { set; get; }
    }
}
