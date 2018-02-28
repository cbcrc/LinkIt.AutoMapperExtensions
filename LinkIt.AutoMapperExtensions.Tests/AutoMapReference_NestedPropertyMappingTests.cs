#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using AutoMapper;
using NUnit.Framework;

namespace LinkIt.AutoMapperExtensions.Tests
{
    [TestFixture]
    public class AutoMapReference_NestedPropertyMappingTests
    {
        [Test]
        public void MapModel_WithNestedPropertiesMappingForModelProperty_ShouldMap()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MyPoint, MyPointDto>();

                cfg.CreateMap<MyNestedLinkedSource, MyNestedDto>()
                    .MapLinkedSource(x => x.Model.Property);
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var source = new MyNestedLinkedSource
            {
                Model = CreateMyNestedModel(1),
                Point = new MyPoint { X = 1, Y = 2 }
            };

            var actual = mapper.Map<MyNestedDto>(source);

            Assert.AreEqual(100, actual.Number);
            Assert.AreEqual($"Title {actual.Number}", actual.Title);
            Assert.AreEqual($"Text {actual.Number}", actual.Text);
            Assert.AreEqual(1, actual.Point.X);
            Assert.AreEqual(2, actual.Point.Y);
        }

        private static MyNestedModel CreateMyNestedModel(int id)
        {
            var propId = id * 100;
            return new MyNestedModel
            {
                Id = id,
                Title = $"Title {id}",
                Property = new MyNestedProperty
                {
                    Number = propId,
                    Title = $"Title {propId}",
                    Text = $"Text {propId}"
                }
            };
        }

        public class MyNestedLinkedSource
        {
            public MyNestedModel Model { get; set; }
            public MyPoint Point { get; set; }
        }

        public class MyPoint {
            public float X { get; set; }
            public float Y { get; set; }
        }

        public class MyNestedModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public MyNestedProperty Property { get; set; }
        }

        public class MyNestedProperty
        {
            public int Number { get; set; }
            public string Title { get; set; }
            public string Text { get; set; }
        }

        public class MyNestedDto
        {
            public string Title { get; set; }
            public int Number { get; set; }
            public string Text { get; set; }
            public MyPointDto Point { get; set; }
        }

        public class MyPointDto {
            public float X { get; set; }
            public float Y { get; set; }
        }
    }
}