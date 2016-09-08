#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using System;
using NUnit.Framework;

namespace LinkIt.AutoMapperExtensions.Tests
{
    [TestFixture]
    public class OverrideConventionTests
    {
        [Test]
        public void IsOverridden_WithNullabeT()
        {
            int? x = 1;
            Assert.That(OverrideConvention.IsOverridden(x), Is.True);
            int? y = null;
            Assert.That(OverrideConvention.IsOverridden(y), Is.False);
        }

        [Test]
        public void IsOverridden_WithNullabeTAsObject() {
            int? x = 1;
            Assert.That(OverrideConvention.IsOverridden((object)x), Is.True);
            int? y = null;
            Assert.That(OverrideConvention.IsOverridden((object)y), Is.False);
        }

        [Test]
        public void IsOverridden_WithObject() {
            Uri x = new Uri("http://google.com");
            Assert.That(OverrideConvention.IsOverridden(x), Is.True);
            Uri y = null;
            Assert.That(OverrideConvention.IsOverridden(y), Is.False);
        }


        [Test]
        public void IsOverridden_WithStrings() {
            string x = "http://google.com";
            Assert.That(OverrideConvention.IsOverridden(x), Is.True);
            string y = null;
            Assert.That(OverrideConvention.IsOverridden(y), Is.False);
            string y1 = "";
            Assert.That(OverrideConvention.IsOverridden(y1), Is.False);
            string y2 = "  ";
            Assert.That(OverrideConvention.IsOverridden(y2), Is.False);

        }

    }
}
