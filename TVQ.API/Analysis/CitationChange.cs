namespace Genometric.TVQ.API.Analysis
{
    public class CitationChange
    {
        public double DaysOffset { set; get; }
        public double CitationCount { set; get; }

        public CitationChange(double daysOffset, double count)
        {
            DaysOffset = daysOffset;
            CitationCount = count;
        }
    }
}
