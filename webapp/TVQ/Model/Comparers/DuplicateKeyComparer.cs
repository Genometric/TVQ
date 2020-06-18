using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Genometric.TVQ.API.Model.Comparers
{
    public class DuplicateKeyComparer<T> : IComparer<T>
        where T : IComparable
    {
        public int Compare([AllowNull] T x, [AllowNull] T y)
        {
            int defaultComparison = x.CompareTo(y);

            return defaultComparison == 0 ? 1 : defaultComparison;
        }
    }
}
