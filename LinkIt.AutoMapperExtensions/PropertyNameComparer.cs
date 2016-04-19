#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

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