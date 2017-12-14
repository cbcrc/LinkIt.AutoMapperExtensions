#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using System;
using Xunit;

namespace LinkIt.AutoMapperExtensions.Tests
{
    public class OverrideConventionTests
    {
        [Fact]
        public void IsOverridden_WithNullabeT()
        {
            int? x = 1;
            Assert.True(OverrideConvention.IsOverridden(x));
            int? y = null;
            Assert.False(OverrideConvention.IsOverridden(y));
        }

        [Fact]
        public void IsOverridden_WithNullabeTAsObject() {
            int? x = 1;
            Assert.True(OverrideConvention.IsOverridden((object)x));
            int? y = null;
            Assert.False(OverrideConvention.IsOverridden((object)y));
        }

        [Fact]
        public void IsOverridden_WithObject() {
            Uri x = new Uri("http://google.com");
            Assert.True(OverrideConvention.IsOverridden(x));
            Uri y = null;
            Assert.False(OverrideConvention.IsOverridden(y));
        }


        [Fact]
        public void IsOverridden_WithStrings() {
            string x = "http://google.com";
            Assert.True(OverrideConvention.IsOverridden(x));
            string y = null;
            Assert.False(OverrideConvention.IsOverridden(y));
            string y1 = "";
            Assert.False(OverrideConvention.IsOverridden(y1));
            string y2 = "  ";
            Assert.False(OverrideConvention.IsOverridden(y2));

        }

    }
}
