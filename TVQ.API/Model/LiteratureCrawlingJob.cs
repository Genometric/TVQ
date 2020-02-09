using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    public class LiteratureCrawlingJob : BaseJob
    {
        public bool ScanAllPublications { set; get; } = false;

        // TODO: this should not be correct! 
        // This is a many-to-many relation and should not be implemented this.
        public virtual List<Publication> Publications { set; get; }
    }
}
