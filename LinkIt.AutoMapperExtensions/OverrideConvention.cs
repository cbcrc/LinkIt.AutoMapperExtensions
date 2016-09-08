#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion
namespace LinkIt.AutoMapperExtensions
{
    public static class OverrideConvention
    {
        public static bool IsOverridden<T>(T overridingValue) {
            if (typeof(T) == typeof(string)) {
                var asString = (string)(object)overridingValue;
                return !string.IsNullOrWhiteSpace(asString);
            }

            return overridingValue != null;
        }
    }
}
