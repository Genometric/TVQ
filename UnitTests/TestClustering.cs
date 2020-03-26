using Genometric.TVQ.API.Analysis.Clustering;
using Xunit;

namespace Genometric.TVQ.UnitTests
{
    public class TestClustering
    {
        [Fact]
        public void BasicClustering()
        {
            var alg = new AgglomerativeHierarchicalClustering();
            var input = new double[,]
            {
                {0, 0, 0, 0 },
                {0, 1, 0, 1 },
                {9, 9, 9, 9 }
            };

            string[] ids = new string[] { "1", "2", "8" };

            var cluster = alg.Cluster(input, ids, DistanceMetric.Euclidean, new SingleLinkageStrategy());

            Assert.True(cluster.Children.Count == 2);
            Assert.True(cluster.Dist == 17.029386365926403);
            Assert.True(cluster.LeafNames.Count == 3);
            Assert.True(cluster.Parent == null);
            Assert.True(cluster.CountLeaves() == 3);
            Assert.Contains("1", cluster.LeafNames);
            Assert.Contains("2", cluster.LeafNames);
            Assert.Contains("8", cluster.LeafNames);

            Assert.True(cluster.Children[0].CountLeaves() == 1);
            Assert.True(cluster.Children[0].Name == "8");
            Assert.True(cluster.Children[0].LeafNames.Count == 1);
            Assert.True(cluster.Children[0].LeafNames[0] == "8");
            Assert.True(cluster.Children[0].Children.Count == 0);
            Assert.True(cluster.Children[0].Dist == 0);

            Assert.True(cluster.Children[1].CountLeaves() == 2);
            Assert.True(cluster.Children[1].Name == "C#1");
            Assert.True(cluster.Children[1].LeafNames.Count == 2);
            Assert.True(cluster.Children[1].LeafNames[0] == "1");
            Assert.True(cluster.Children[1].LeafNames[1] == "2");
            Assert.True(cluster.Children[1].Children.Count == 2);
            Assert.True(cluster.Children[1].Dist == 1.4142135623730951);
        }
    }
}
