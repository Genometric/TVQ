using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Genometric.TVQ.API.Analysis.Clustering
{
    public class ClusterNode : IComparable<ClusterNode>
    {
        public string Name { set; get; }
        public ClusterNode Parent { set; get; }
        public ClusterNode Left { set; get; }
        public ClusterNode Right { set; get; }

        public List<string> LeafNames { set; get; } = new List<string>();
        public List<ClusterNode> Children { set; get; } = new List<ClusterNode>();

        public double Distance { set; get; } = double.NaN;
        public double Weight { set; get; } = 1.0;

        public ClusterNode() { }

        public ClusterNode(string name)
        {
            Name = name;
        }

        public ClusterNode GetOtherCluster(ClusterNode c)
        {
            return Left == c ? Right : Left;
        }

        public ClusterNode Agglomerate(int clusterIdx)
        {
            return Agglomerate("C#" + clusterIdx);
        }

        public ClusterNode Agglomerate(string name)
        {
            var cluster = new ClusterNode(name)
            {
                Distance = Distance
            };

            cluster.LeafNames.AddRange(Left.LeafNames);
            cluster.LeafNames.AddRange(Right.LeafNames);
            cluster.Children.Add(Left);
            cluster.Children.Add(Right);
            Left.Parent = cluster;
            Right.Parent = cluster;

            double lWeight = Left.Weight;
            double rWeight = Right.Weight;
            double weight = lWeight + rWeight;
            cluster.Weight = weight;

            return cluster;
        }

        public int CompareTo([AllowNull] ClusterNode other)
        {
            return other == null ? -1 : Distance.CompareTo(other.Distance);
        }

        public bool IsLeaf()
        {
            return Children.Count == 0;
        }

        public int CountLeaves(int count = 0)
        {
            if (IsLeaf())
                count++;

            foreach (var child in Children)
                count += child.CountLeaves();

            return count;
        }

        public string ToNewickString(int indent)
        {
            var builder = new StringBuilder("");
            if (!IsLeaf())
                builder.Append("(");

            for (int i = 0; i < indent; i++)
                builder.Append(" ");

            if (IsLeaf())
                builder.Append(Name);

            var children = Children;

            bool firstChild = true;
            foreach (var child in children)
            {
                builder.Append(child.ToNewickString(indent));
                if (firstChild)
                    builder.Append(":" + Distance.ToString().Replace(",", ".") + ",");
                else
                    builder.Append(":" + Weight.ToString().Replace(",", "."));

                firstChild = false;
            }

            for (int i = 0; i < indent; i++)
                builder.Append(" ");

            if (!IsLeaf())
                builder.Append(")");

            return builder.ToString();
        }
    }
}
