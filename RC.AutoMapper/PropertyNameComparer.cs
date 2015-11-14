using System.Collections.Generic;
using System.Reflection;

namespace LinkIt.AutoMapperExtensions
{
    public class PropertyNameComparer : IEqualityComparer<PropertyInfo> {
        public bool Equals(PropertyInfo x, PropertyInfo y) {
            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(PropertyInfo obj) {
            return obj.Name.GetHashCode();
        }
    }
}