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
        /// <returns>If means of <paramref name="x"/> and <paramref name="y"/>
        /// are significantly different.</returns>
        public static bool ComputeTTest(
            List<double> x,
            List<double> y,
            double significanceLevel,
            out double df,
            out double tScore,
            out double pValue,
            out double criticalValue)
        {
            var aMean = Statistics.Mean(x);
            var aVari = Statistics.Variance(x);

            var bMean = Statistics.Mean(y);
            var bVari = Statistics.Variance(y);

            tScore =
                (aMean - bMean) /
                Math.Sqrt((aVari / x.Count) + (bVari / y.Count));

            df = Math.Round(
                Math.Pow((aVari / x.Count) + (bVari / y.Count), 2) /
                ((Math.Pow(aVari, 2) / (Math.Pow(x.Count, 2) * (x.Count - 1))) +
                (Math.Pow(bVari, 2) / (Math.Pow(y.Count, 2) * (y.Count - 1)))));

            pValue = 2.0D * (1 - StudentT.CDF(0.0, 1.0, df, Math.Abs(tScore)));
            criticalValue = StudentT.InvCDF(0.0, 1.0, df, 1.0 - significanceLevel / 2.0);
            return tScore > criticalValue;
        }
    }
}
