using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Genometric.TVQ.API.Analysis.Clustering
{
    public class AgglomerativeHierarchicalClustering
    {
        private DistanceMap _distances;
        private List<ClusterNode> _clusters;
        private int _globalClusterIndex = 0;

        /// <summary>
        /// Perform hierarchical/agglomerative clustering.
        /// </summary>
        /// <param name="x">An m by n array of m original observations in an n-dimensional space.</param>
        /// <param name="labels"></param>
        /// <param name="metric">The distance metric to use.</param>
        /// <param name="linkage"></param>
        /// <returns>Return root cluster.</returns>
        public ClusterNode Cluster(double[,] x, string[] labels, DistanceMetric metric, ILinkageStrategy linkage)
        {
            var pdist = GetPDist(x, metric);
            _clusters = CreateClusters(labels);
            _distances = CreateLinkages(pdist, _clusters);

            while (_clusters.Count != 1)
                Agglomerate(linkage);

            return _clusters[0];
        }

        /// <summary>
        /// Returns pairwise distances between observations in n-dimensional space.
        /// </summary>
        /// <param name="space"></param>
        /// <returns></returns>
        private static double[,] GetPDist(double[,] space, DistanceMetric metric)
        {
            // Gets a distance method to be used based on the method name.
            var distanceMethod = GetDistanceMethod(metric);

            int rowCount = space.GetLength(0);
            int colCount = space.GetLength(1);
            int rtvIndex = 0;

            var pDist = new double[1, (int)Math.Floor((Math.Pow(rowCount, 2) - rowCount) / 2.0)];

            for (int i = 0; i < rowCount - 1; i++)
                for (int j = i + 1; j < rowCount; j++)
                {
                    var rowA = Enumerable.Range(0, space.GetLength(1)).Select(x => space[i, x]).ToArray();
                    var rowB = Enumerable.Range(0, space.GetLength(1)).Select(x => space[j, x]).ToArray();

                    pDist[0, rtvIndex++] = (double)distanceMethod.Invoke(null, new object[] { rowA, rowB });
                }

            return pDist;
        }

        public void Agglomerate(ILinkageStrategy linkageStrategy)
        {
            ClusterNode minDistLink = _distances.RemoveFirst();
            if (minDistLink != null)
            {
                _clusters.Remove(minDistLink.Right);
                _clusters.Remove(minDistLink.Left);

                var newCluster = minDistLink.Agglomerate(++_globalClusterIndex);

                foreach (var c in _clusters)
                {
                    var newLinkage = new ClusterNode
                    {
                        Left = c,
                        Right = newCluster
                    };

                    var dists = new List<double>();
                    var linkA = _distances.Remove(c, minDistLink.Left);
                    var linkB = _distances.Remove(c, minDistLink.Right);

                    if (linkA != null)
                    {
                        dists.Add(linkA.LinkageDistance);

                    }
                    if (linkB != null)
                    {
                        dists.Add(linkB.LinkageDistance);

                        // double weightVal = link2.GetOtherCluster(iClust).Weight;
                    }

                    newLinkage.LinkageDistance = linkageStrategy.CalculateDistance(dists);
                    _distances.Add(newLinkage);
                }
                _clusters.Add(newCluster);
            }
        }

        private static MethodInfo GetDistanceMethod(DistanceMetric metric)
        {
            Type[] methodTypes;
            if (metric == DistanceMetric.Pearson)
                methodTypes = new Type[] { typeof(IEnumerable<double>), typeof(IEnumerable<double>) };
            else
                methodTypes = new Type[] { typeof(double[]), typeof(double[]) };

            return typeof(MathNet.Numerics.Distance).GetMethod(metric.ToString(), methodTypes);
        }

        private DistanceMap CreateLinkages(double[,] distances, List<ClusterNode> clusters)
        {
            var linkages = new DistanceMap();
            for (int col = 0; col < clusters.Count; col++)
            {
                for (int row = col + 1; row < clusters.Count; row++)
                {
                    var link = new ClusterNode
                    {
                        LinkageDistance = distances[0, AccessFunction(row, col, clusters.Count)],
                        Left = clusters[col],
                        Right = clusters[row]
                    };

                    linkages.Add(link);
                }
            }
            return linkages;
        }

        private List<ClusterNode> CreateClusters(string[] clusterNames)
        {
            var clusters = new List<ClusterNode>();
            foreach (var name in clusterNames)
            {
                var cluster = new ClusterNode(name);
                cluster.LeafNames.Add(name);
                clusters.Add(cluster);
            }
            return clusters;
        }

        private static int AccessFunction(int i, int j, int n)
        {
            return n * j - j * (j + 1) / 2 + i - 1 - j;
        }
    }
}
