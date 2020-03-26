using System.Collections.Generic;

namespace Genometric.TVQ.API.Analysis.Clustering
{
    public interface ILinkageStrategy
    {
        public double CalculateDistance(IEnumerable<double> distances);
    }
}
