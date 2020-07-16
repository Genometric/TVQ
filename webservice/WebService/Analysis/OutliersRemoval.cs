using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genometric.TVQ.WebService.Analysis
{
    // ---------------------------------
    // All the methods are experimental.
    // ---------------------------------

    public static class OutliersRemoval
    {

        /// <summary>
        /// Dispersion reduction using interquartile range.
        /// Implemented based on: https://www.mathworks.com/matlabcentral/cody/problems/42485
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static SortedSet<double> Remove(SortedSet<double> input)
        {
            if (input == null)
                return new SortedSet<double>();

            while (input.Count > 0)
            {
                var meanValue = input.Average();
                var furthestFromMean = input.OrderByDescending(x => Math.Abs(x - meanValue)).First();
                var iqr = Statistics.UpperQuartile(input) - Statistics.LowerQuartile(input);
                double maxRange = 1.5 * iqr;

                if (Math.Abs(furthestFromMean - meanValue) <= maxRange)
                    return input;

                input.Remove(furthestFromMean);
            }

            return new SortedSet<double>();
        }
    }
}
