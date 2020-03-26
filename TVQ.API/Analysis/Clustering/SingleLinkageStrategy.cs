using System.Collections.Generic;

namespace Genometric.TVQ.API.Analysis.Clustering
{
	public class SingleLinkageStrategy : ILinkageStrategy
	{
		public double CalculateDistance(IEnumerable<double> distances)
		{
			var min = double.NaN;
			foreach (var dist in distances)
				if (double.IsNaN(min) || dist < min)
					min = dist;
			return min;
		}
	}
}
