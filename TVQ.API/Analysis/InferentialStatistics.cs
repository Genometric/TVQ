using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Analysis
{
    public static class InferentialStatistics
    {
        /// <summary>
        /// Computes Welch's t-test.
        /// </summary>
        /// <param name="x">First population.</param>
        /// <param name="y">Second population.</param>
        /// <returns>p-value</returns>
        public static bool TryComputeTTest(List<double> x, List<double> y, out double tScore, out double pValue)
        {
            tScore = double.NaN;
            pValue = double.NaN;

            if (x == null || y == null)
                return false;

            var aMean = Statistics.Mean(x);
            var aVari = Statistics.Variance(x);

            var bMean = Statistics.Mean(y);
            var bVari = Statistics.Variance(y);

            var t = 
                (aMean - bMean) / 
                Math.Sqrt((aVari / x.Count) + (bVari / y.Count));

            var df =
                Math.Round(
                    Math.Pow((aVari / x.Count) + (bVari / y.Count), 2) /
                    ((Math.Pow(aVari, 2) / (Math.Pow(x.Count, 2) * (x.Count - 1))) +
                    (Math.Pow(bVari, 2) / (Math.Pow(y.Count, 2) * (y.Count - 1)))));

            var distribution = new StudentT(0.0, 1.0, df);

            tScore = 2.0D * distribution.CumulativeDistribution(t);
            pValue = 2.0D * (1 - distribution.CumulativeDistribution(Math.Abs(tScore)));

            return true;
        }
    }
}
