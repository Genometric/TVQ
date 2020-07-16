using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Genometric.TVQ.WebService.Model.JsonConverters
{
    public class PropertyComparer : IComparer<PropertyInfo>
    {
        public int Compare([AllowNull] PropertyInfo x, [AllowNull] PropertyInfo y)
        {
            if (x == null)
                return 1;
            
            if (y == null)
                return -1;
            
            if (x == null && y == null)
                return 0;

            if (x.Name == nameof(BaseModel.ID))
                return -1;

            if (y.Name == nameof(BaseModel.ID))
                return 1;

            if (x.Name.EndsWith("ID", StringComparison.InvariantCulture) &&
                !y.Name.EndsWith("ID", StringComparison.InvariantCulture))
                return -1;

            if (!x.Name.EndsWith("ID", StringComparison.InvariantCulture) &&
                y.Name.EndsWith("ID", StringComparison.InvariantCulture))
                return 1;

            if (x.Name.EndsWith("ID", StringComparison.InvariantCulture) &&
                y.Name.EndsWith("ID", StringComparison.InvariantCulture))
                return string.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase);

            if (x.GetGetMethod().IsVirtual && !y.GetGetMethod().IsVirtual)
                return 1;

            if (!x.GetGetMethod().IsVirtual && y.GetGetMethod().IsVirtual)
                return -1;

            if (x.GetType().GetInterface(nameof(IEnumerable)) != null &&
                y.GetType().GetInterface(nameof(IEnumerable)) == null)
                return 1;

            if (x.GetType().GetInterface(nameof(IEnumerable)) == null &&
                y.GetType().GetInterface(nameof(IEnumerable)) != null)
                return -1;

            if (x.Name == nameof(BaseModel.CreatedDate))
                return 1;

            if (y.Name == nameof(BaseModel.CreatedDate))
                return -1;

            if (x.Name == nameof(BaseModel.UpdatedDate))
                return 1;

            if (y.Name == nameof(BaseModel.UpdatedDate))
                return -1;

            return string.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
