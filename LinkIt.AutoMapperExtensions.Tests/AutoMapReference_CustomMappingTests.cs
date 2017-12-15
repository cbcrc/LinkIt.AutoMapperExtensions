#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using AutoMapper;
using Xunit;

namespace LinkIt.AutoMapperExtensions.Tests
{
    public class AutoMapReference_CustomMappingTests
    {
        [Fact]
        public void MapModel_WithCustomMappingForOtherProperty_ShouldMap()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg
                    .CreateMap<MyCustomLinkedSource, MyCustomDto>()
                    .MapLinkedSource()
                    .ForMember(destination => destination.SelfUrl, opt => opt.MapFrom(src => "http://blah.com/" + src.Model.Id));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var source = new MyCustomLinkedSource
            {
                Model = CreateMyCustomModel(1),
            };

            var actual = mapper.Map<MyCustomDto>(source);

            Assert.Equal("http://blah.com/" + source.Model.Id, actual.SelfUrl);
        }

        [Fact]
        public void MapModel_WithCustomMappingForModelProperty_ShouldMap()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg
                    .CreateMap<MyCustomLinkedSource, MyCustomDto>()
                    .MapLinkedSource()
                    .ForMember(destination => destination.Title, opt => opt.MapFrom(src => src.Model.Title + " TEST"))
                    .ForMember(destination => destination.SelfUrl, opt => opt.Ignore());
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var source = new MyCustomLinkedSource
            {
                Model = CreateMyCustomModel(1),
            };

            var actual = mapper.Map<MyCustomDto>(source);

            Assert.Equal(source.Model.Title + " TEST", actual.Title);
        }

        private static MyCustomModel CreateMyCustomModel(int id)
        {
            return new MyCustomModel{
                Id = id,
                Title = "Title "+id,
            };
        }

        public class MyCustomLinkedSource
        {
            public MyCustomModel Model { get; set; }
        }

        public class MyCustomModel {
            public int Id { get; set; }
            public string Title { get; set; }
        }

        public class MyCustomDto {
            public string SelfUrl { get; set; }
            public string Title { get; set; }
        }


    }
}