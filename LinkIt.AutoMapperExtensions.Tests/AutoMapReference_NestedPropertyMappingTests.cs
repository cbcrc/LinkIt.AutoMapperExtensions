#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using AutoMapper;
using Xunit;

namespace LinkIt.AutoMapperExtensions.Tests
{
    public class AutoMapReference_NestedPropertyMappingTests
    {
        [Fact]
        public void MapModel_WithNestedPropertiesMappingForModelProperty_ShouldMap()
        { 
            var config = new MapperConfiguration(cfg =>
            {
                cfg
                    .CreateMap<MyNestedLinkedSource, MyNestedDto>()
                    .MapNestedProperties(x => x.Model.Property);
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var source = new MyNestedLinkedSource
            {
                Model = CreateMyNestedModel(1),
            };

            var actual = mapper.Map<MyNestedDto>(source);

            Assert.Equal(100, actual.Number);
            Assert.Equal($"Title {actual.Number}", actual.Title);
            Assert.Equal($"Text {actual.Number}", actual.Text);
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
        }
    }
}