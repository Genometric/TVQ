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

        public SortedDictionary<double, double> CitationsVector { get; }

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
            CitationsVector = new SortedDictionary<double, double>();
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
            Normalize();
            Interpolate(points);
        }

        private void AddRange(
            ICollection<Citation> citations, 
            DateTime? dateAddedToRepository)
        {
            double daysOffset;
            foreach (var citation in citations)
            {
                daysOffset = (citation.Date - dateAddedToRepository).Value.Days;
                if (CitationsVector.ContainsKey(daysOffset))
                    CitationsVector[daysOffset] += citation.Count;
                else
                    CitationsVector.Add(daysOffset,
                        citation.Count);
            }
        }

        private void Normalize()
        {
            var minBeforeDays = CitationsVector.First().Key;
            var maxAfterDays = CitationsVector.Last().Key;
            var delta = maxAfterDays - minBeforeDays;

            // Normalize citation count.
            var days = new double[CitationsVector.Keys.Count];
            CitationsVector.Keys.CopyTo(days, 0);
            foreach (var day in days)
                CitationsVector[day] /= delta;

            // Min-Max normalize date.
            foreach (var day in days)
            {
                double normalizedDate;
                if (day <= 0)
                    normalizedDate = (-1) * ((day - maxAfterDays) / (minBeforeDays - maxAfterDays));
                else
                    normalizedDate = (day - minBeforeDays) / (maxAfterDays - minBeforeDays);

                CitationsVector.Add(normalizedDate, CitationsVector[day]);
                CitationsVector.Remove(day);
            }
        }

        private void Interpolate(IEnumerable<double> points)
        {
            // At least two items are required for interpolation.
            if (CitationsVector.Count < 2)
                return;

            var spline = MathNet.Numerics.Interpolate.Linear(
                CitationsVector.Keys, CitationsVector.Values);

            CitationsVector.Clear();
            foreach(var x in points)
            {
                var y = spline.Interpolate(x);
                if (y < 0) y = 0;

                CitationsVector.Add(x, y);
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
