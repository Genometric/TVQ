namespace Genometric.TVQ.API.Analysis
{
    public struct CitationChange
    {
        public int DaysOffset { get; }
        public double Count { get; }

        public CitationChange(int daysOffset, double count)
        {
            DaysOffset = daysOffset;
            Count = count;
        }
    }
}
