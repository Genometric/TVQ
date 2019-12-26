using Genometric.TVQ.API.Analysis;
using System.Collections.Generic;
using Xunit;

namespace Genometric.TVQ.UnitTests
{
    public class TestInferentialStats
    {
        [Fact]
        public void ComputeTScoreAndPValue()
        {
            // Arrange
            var x = new List<double>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var y = new List<double>() { 1, 2, 3, 4, 50, 600, 7000, 80000, 900000 };

            // Act
            var sigDiff = InferentialStatistics.ComputeTTest(
                x, y, 0.05,
                out double df,
                out double tScore, 
                out double pValue, 
                out double criticalValue);

            // Arrange
            Assert.Equal(-1.1065608371837756, tScore);
            Assert.Equal(0.30064508991068917, pValue);
            Assert.Equal(8, df);
            Assert.False(sigDiff);
            Assert.Equal(2.306004135204172, criticalValue);
        }
    }
}
