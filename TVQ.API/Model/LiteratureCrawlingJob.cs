using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    public class LiteratureCrawlingJob : BaseJob
    {
        public bool ScanAllPublications { set; get; } = false;
        public virtual List<Publication> Publications { set; get; }
    }
}
