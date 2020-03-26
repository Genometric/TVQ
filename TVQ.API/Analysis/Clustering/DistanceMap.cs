using System.Collections.Generic;
using System.Linq;

namespace Genometric.TVQ.API.Analysis.Clustering
{
    public class DistanceMap
    {
        private readonly Dictionary<string, ClusterNode> _hashes;
        private readonly SortedDictionary<string, ClusterNode> _nodes;

        public DistanceMap()
        {
            _nodes = new SortedDictionary<string, ClusterNode>();
            _hashes = new Dictionary<string, ClusterNode>();
        }

        public ClusterNode RemoveFirst()
        {
            var poll = Pop();
            while (!poll.Equals(default(KeyValuePair<string, ClusterNode>)) && poll.Value == null)
                poll = Pop();

            if (poll.Equals(default(KeyValuePair<string, ClusterNode>)))
                return null;

            var link = poll.Value;
            _hashes.Remove(poll.Key);
            return link;
        }

        private KeyValuePair<string, ClusterNode> Pop()
        {
            var item = _nodes.FirstOrDefault();
            _nodes.Remove(item.Key);
            return item;
        }

        public void Remove(ClusterNode node)
        {
            if (node == null)
                return;

            var key = GetHash(node);
            _hashes.Remove(GetHash(node), out ClusterNode item);
            if (item == null)
                return;
            _nodes[key] = null;
        }

        public ClusterNode Remove(ClusterNode x, ClusterNode y)
        {
            if (_hashes.TryGetValue(GetHash(x, y), out ClusterNode node))
                Remove(node);
            return node;
        }

        public bool Add(ClusterNode node)
        {
            var key = GetHash(node);
            if (_hashes.TryGetValue(key, out ClusterNode item))
            {
                return false;
            }
            else
            {
                _hashes.Add(key, node);
                _nodes.Add(key, node);
                return true;
            }
        }

        public static string GetHash(ClusterNode node)
        {
            return GetHash(node.Left, node.Right);
        }

        public static string GetHash(ClusterNode x, ClusterNode y)
        {
            var delimiter = ":::";
            if (x.Name.CompareTo(y.Name) < 0)
                return x.Name + delimiter + y.Name;
            else
                return y.Name + delimiter + x.Name;
        }
    }
}
