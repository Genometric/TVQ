using System.Collections.Generic;

namespace Genometric.TVQ.API.Model.JsonConverters
{
    public class StatisticsJsonConverter : BaseJsonConverter
    {
        public StatisticsJsonConverter() : base(
            propertyMappings: new Dictionary<string, string>
            {
                {"tScore", nameof(Statistics.TScore)},
                {"pValue", nameof(Statistics.PValue)},
                {"degreeOfFreedom", nameof(Statistics.DegreeOfFreedom)},
                {"criticalValue", nameof(Statistics.CriticalValue)},
                {"meansSignificantlyDifferent", nameof(Statistics.MeansSignificantlyDifferent)}
            },
            includeNullProperties: true)
        { }
    }
}
