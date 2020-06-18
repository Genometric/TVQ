using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Analysis
{
    public static class InferentialStatistics
    {
        public static double GetPValue(double tScore, double df)
        {
            return 2.0D * (1 - StudentT.CDF(0.0, 1.0, df, Math.Abs(tScore)));
        }

        public static double GetCriticalValue(double significanceLevel, double df)
        {
            return StudentT.InvCDF(0.0, 1.0, df, 1.0 - significanceLevel / 2.0);
        }

        public static bool IsSignificant(double tScore, double criticalValue)
        {
            return tScore > criticalValue;
        }

        /// <summary>
        /// Computes Welch's t-test.
        /// </summary>
        /// <param name="x">First population.</param>
        /// <param name="y">Second population.</param>
        /// <returns>If means of <paramref name="x"/> and <paramref name="y"/>
        /// are significantly different.</returns>
        public static bool? ComputeTTest(
            List<double> x,
            List<double> y,
            double significanceLevel,
            out double df,
            out double tScore,
            out double pValue,
            out double criticalValue,
            bool doubleSide = true)
        {
            df = double.NaN;
            tScore = double.NaN;
            pValue = double.NaN;
            criticalValue = double.NaN;
            if (x.Count == 0 || y.Count == 0)
                return null;

            var aMean = Statistics.Mean(x);
            var aVari = Statistics.Variance(x);

            var bMean = Statistics.Mean(y);
            var bVari = Statistics.Variance(y);

            tScore =
                (aMean - bMean) /
                Math.Sqrt((aVari / x.Count) + (bVari / y.Count));

            if (!doubleSide)
                tScore = Math.Abs(tScore);

            df = Math.Round(
                Math.Pow((aVari / x.Count) + (bVari / y.Count), 2) /
                ((Math.Pow(aVari, 2) / (Math.Pow(x.Count, 2) * (x.Count - 1))) +
                (Math.Pow(bVari, 2) / (Math.Pow(y.Count, 2) * (y.Count - 1)))));

            pValue = GetPValue(tScore, df);
            criticalValue = GetCriticalValue(significanceLevel, df);
            return IsSignificant(tScore, criticalValue);
        }


        /// <summary>
        /// Paired t-test. 
        /// </summary>
        public static bool? ComputeTTest(
            List<double> deltas,
            double significanceLevel,
            out double df,
            out double tScore,
            out double pValue,
            out double criticalValue)
        {
            var mean = Statistics.Mean(deltas);
            var stdv = Statistics.StandardDeviation(deltas);
            tScore = mean / (stdv / Math.Sqrt(deltas.Count));

            df = deltas.Count - 1;
            pValue = GetPValue(tScore, df);
            criticalValue = GetCriticalValue(significanceLevel, df);
            return IsSignificant(tScore, criticalValue);
        }
    }
}
