using Genometric.TVQ.API.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Genometric.TVQ.API.Analysis
{
    public class CitationChange
    {
        public double DaysOffset { set; get; }

        public double YearsOffset { set; get; }

        public double CitationCount { set; get; }

        public double Average { set; get; }

        private SortedSet<double> Citations { set; get; }

        public SortedDictionary<double, double> CitationsVectorNormalizedByDays { get; }

        public SortedDictionary<double, double> CitationsVectorNormalizedByYears { get; }

        public double Min
        {
            get { return MathNet.Numerics.Statistics.Statistics.Minimum(Citations); }
        }

        public double Max
        {
            get { return MathNet.Numerics.Statistics.Statistics.Maximum(Citations); }
        }

        public double Median
        {
            get { return MathNet.Numerics.Statistics.Statistics.Median(Citations); }
        }

        public double LowerQuartile
        {
            get { return MathNet.Numerics.Statistics.Statistics.LowerQuartile(Citations); }
        }

        public double UpperQuartile
        {
            get { return MathNet.Numerics.Statistics.Statistics.UpperQuartile(Citations); }
        }

        public CitationChange()
        {
            Citations = new SortedSet<double>();
            CitationsVectorNormalizedByDays = new SortedDictionary<double, double>();
            CitationsVectorNormalizedByYears = new SortedDictionary<double, double>();
        }

        public CitationChange(double daysOffset, double count)
        {
            DaysOffset = daysOffset;
            CitationCount = count;
            Citations = new SortedSet<double>();
        }

        public void AddRange(
            ICollection<Citation> citations, 
            DateTime? dateAddedToRepository, 
            IEnumerable<double> points)
        {
            Contract.Requires(citations != null);
            Contract.Requires(dateAddedToRepository != null);
            Contract.Requires(points != null);

            AddRange(citations, dateAddedToRepository);

            Normalize(CitationsVectorNormalizedByDays);
            Normalize(CitationsVectorNormalizedByYears);

            Interpolate(CitationsVectorNormalizedByDays, points);
            Interpolate(CitationsVectorNormalizedByYears, points);
        }

        private void AddRange(
            ICollection<Citation> citations, 
            DateTime? dateAddedToRepository)
        {
            double daysOffset;
            double yearsOffset;
            foreach (var citation in citations)
            {
                daysOffset = (citation.Date - dateAddedToRepository).Value.Days;
                if (CitationsVectorNormalizedByDays.ContainsKey(daysOffset))
                    CitationsVectorNormalizedByDays[daysOffset] += citation.Count;
                else
                    CitationsVectorNormalizedByDays.Add(daysOffset,
                        citation.Count);

                // The average number of days per year is 365 + ​1⁄4 − ​1⁄100 + ​1⁄400 = 365.2425
                // REF: https://en.wikipedia.org/wiki/Leap_year
                yearsOffset = (citation.Date - dateAddedToRepository).Value.TotalDays / 365.2425;
                if (CitationsVectorNormalizedByYears.ContainsKey(yearsOffset))
                    CitationsVectorNormalizedByYears[yearsOffset] += citation.Count;
                else
                    CitationsVectorNormalizedByYears.Add(yearsOffset,
                        citation.Count);
            }
        }

        private void Normalize(SortedDictionary<double, double> citations)
        {
            var minBeforeDate = citations.First().Key;
            var maxAfterDate = citations.Last().Key;
            var deltaDate = maxAfterDate - minBeforeDate;

            // Normalize citation count.
            var dates = new double[citations.Keys.Count];
            citations.Keys.CopyTo(dates, 0);
            foreach (var date in dates)
                citations[date] /= deltaDate;

            // Min-Max normalize date.
            foreach (var date in dates)
            {
                double normalizedDate;
                if (date <= 0)
                    normalizedDate = (-1) * ((date - maxAfterDate) / (minBeforeDate - maxAfterDate));
                else
                    normalizedDate = (date - minBeforeDate) / (maxAfterDate - minBeforeDate);

                citations.Add(normalizedDate, citations[date]);
                citations.Remove(date);
            }
        }

        private void Interpolate(SortedDictionary<double, double> citations, IEnumerable<double> points)
        {
            // At least two items are required for interpolation.
            if (citations.Count < 2)
                return;

            var spline = MathNet.Numerics.Interpolate.Linear(
                citations.Keys, citations.Values);

            citations.Clear();
            foreach(var x in points)
            {
                var y = spline.Interpolate(x);
                if (y < 0) y = 0;

                citations.Add(x, y);
            }
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
