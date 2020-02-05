using MathNet.Numerics.Statistics;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Analysis
{
    public class CitationChange
    {
        public double DaysOffset { set; get; }

        public double YearsOffset { set; get; }

        public double CitationCount { set; get; }

        public double Average { set; get; }

        public double Min
        {
            get { return Statistics.Minimum(Citations); }
        }

        public double Max
        {
            get { return Statistics.Maximum(Citations); }
        }

        public double Median
        {
            get { return Statistics.Median(Citations); }
        }

        public double LowerQuartile
        {
            get { return Statistics.LowerQuartile(Citations); }
        }

        public double UpperQuartile
        {
            get { return Statistics.UpperQuartile(Citations); }
        }

        private SortedSet<double> Citations { set; get; }

        public CitationChange()
        {
            Citations = new SortedSet<double>();
        }

        public CitationChange(double daysOffset, double count)
        {
            DaysOffset = daysOffset;
            CitationCount = count;
            Citations = new SortedSet<double>();
        }

        public void AddCitationCount(double count)
        {
            Citations.Add(count);
        }

        public void AddCitationCount(List<double> counts)
        {
            Citations.UnionWith(counts);
        }

        public void RemoveOutliers()
        {
            Citations = OutliersRemoval.Remove(Citations);
        }

        public void MinMaxNormalize(double min, double max)
        {
            var normalizedCitationCount = new SortedSet<double>();
            foreach (var count in Citations)
                normalizedCitationCount.Add((count - min) / (max - min));
            Citations = normalizedCitationCount;
        }
    }
}
