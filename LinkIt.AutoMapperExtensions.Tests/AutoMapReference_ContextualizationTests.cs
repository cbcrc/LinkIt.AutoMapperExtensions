#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using System;
using AutoMapper;
using Xunit;

namespace LinkIt.AutoMapperExtensions.Tests
{
    public class AutoMapReference_ContextualizationTests
    {
        private MapperConfiguration _config;
        private IMapper _mapper;
        
        public AutoMapReference_ContextualizationTests()
        {
            _config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MyMediaLinkedSource, MyMediaSummaryDto>()
                    .MapLinkedSource()
                    .ForMember(dto => dto.Id, member => member.MapFrom(source => source.Model.Id));
                cfg.CreateMap<MyComplexLinkedSource, MyComplexDto>()
                    .MapLinkedSource();
            });
            _mapper = _config.CreateMapper();
        }

        [Fact]
        public void AssertConfigurationIsValid()
        {
            _config.AssertConfigurationIsValid();
        }


        [Fact]
        public void Map_WithValueContextualization_ShouldContextualizeValue()
        {
            var linkedSource = new MyMediaLinkedSource
            {
                Model = new MyMedia
                {
                    Id = 1,
                    Title = "The title"
                },
                Contextualization = new MyMediaContextualization
                {
                    Id = 1,
                    SeekTimeInSec = 32,
                    Title = "Overridden title"
                }
            };

            var actual = _mapper.Map<MyMediaSummaryDto>(linkedSource);

            Assert.Equal(linkedSource.Contextualization.Title, actual.Title);
        }

        [Fact]
        public void Map_WithNullValueInContextualization_ShouldNotContextualizeValue()
        {
            var linkedSource = new MyMediaLinkedSource
            {
                Model = new MyMedia
                {
                    Id = 1,
                    Title = "The title"
                },
                Contextualization = new MyMediaContextualization
                {
                    Id = 1,
                    SeekTimeInSec = 32,
                    Title = null
                }
            };

            var actual = _mapper.Map<MyMediaSummaryDto>(linkedSource);

            Assert.Equal(linkedSource.Model.Title, actual.Title);
        }

        [Fact]
        public void Map_WithEmtpyOrWhitespaceStringValueInContextualization_ShouldNotContextualizeValue() {
            var linkedSource = new MyMediaLinkedSource {
                Model = new MyMedia {
                    Id = 1,
                    Title = "The title"
                },
                Contextualization = new MyMediaContextualization {
                    Id = 1,
                    SeekTimeInSec = 32,
                    Title = " "
                }
            };

            var actual = _mapper.Map<MyMediaSummaryDto>(linkedSource);

            Assert.Equal(linkedSource.Model.Title, actual.Title);
        }

        [Fact]
        public void Map_WithAdditionInContextualization_ShouldMapAddition()
        {
            var linkedSource = new MyMediaLinkedSource
            {
                Model = new MyMedia
                {
                    Id = 1,
                    Title = "The title"
                },
                Contextualization = new MyMediaContextualization
                {
                    Id = 1,
                    SeekTimeInSec = 32,
                    Title = "Overridden title"
                }
            };

            var actual = _mapper.Map<MyMediaSummaryDto>(linkedSource);

            Assert.Equal(linkedSource.Contextualization.SeekTimeInSec, actual.SeekTimeInSec);
        }

        [Fact]
        public void Map_WithNullContextualization_ShouldNotContextualizeValue()
        {
            var linkedSource = new MyMediaLinkedSource
            {
                Model = new MyMedia
                {
                    Id = 1,
                    Title = "The title"
                },
                Contextualization = null
            };

            var actual = _mapper.Map<MyMediaSummaryDto>(linkedSource);

            Assert.Equal(linkedSource.Model.Title, actual.Title);
        }


        [Fact]
        public void Map_WithNestedContextualizedLinkedSource_ShouldContextualizeValue()
        {
            var linkedSource = new MyComplexLinkedSource
            {
                Model = new MyComplexModel(),
                Media = new MyMediaLinkedSource
                {
                    Model = new MyMedia
                    {
                        Id = 1,
                        Title = "The title"
                    },
                    Contextualization = new MyMediaContextualization()
                    {
                        Id = 1,
                        SeekTimeInSec = 32,
                        Title = "Overridden title"
                    }
                }
            };

            var actual = _mapper.Map<MyComplexDto>(linkedSource);

            Assert.Equal(linkedSource.Media.Contextualization.Title, actual.Media.Title);
        }



        [Fact]
        public void Map_ValueTypeContextualizationWithDefault_ShouldUseDefault() {
            var linkedSource = new MyMediaLinkedSource {
                Model = new MyMedia {
                    Id = 1,
                    Title = "The title",
                    VolumeLevel = 7,
                    BassEq = 5
                },
                Contextualization = new MyMediaContextualization {
                    Id = 1,
                    SeekTimeInSec = 32,
                    Title = "Overridden title",
                    Image = null,
                    VolumeLevel = 0,
                    BassEq = null
                },
                Image = new Uri("http://www.example.com/default.gif")
            };

            var actual = _mapper.Map<MyMediaSummaryDto>(linkedSource);

            Assert.Equal(0, actual.VolumeLevel);
            Assert.Equal(5, actual.BassEq.Value);
        }

        [Fact]
        public void Map_ValueTypeContextualizationWithOverrides_ShouldContextualize() {
            var linkedSource = new MyMediaLinkedSource {
                Model = new MyMedia {
                    Id = 1,
                    Title = "The title",
                    VolumeLevel = 7,
                    BassEq = 5
                },
                Contextualization = new MyMediaContextualization {
                    Id = 1,
                    SeekTimeInSec = 32,
                    Title = "Overridden title",
                    Image = null,
                    VolumeLevel = 1,
                    BassEq = 2
                },
                Image = new Uri("http://www.example.com/default.gif")
            };

            var actual = _mapper.Map<MyMediaSummaryDto>(linkedSource);

            Assert.Equal(1, actual.VolumeLevel);
            Assert.Equal(2, actual.BassEq);
        }

        public class MyMediaLinkedSource
        {
            public MyMedia Model { get; set; }
            public MyMediaContextualization Contextualization { get; set; }
            public Uri Image { get; set; }
        }

        public class MyMedia
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public int VolumeLevel { get; set; }
            public int? BassEq { get; set; }
        }

        public class MyMediaContextualization
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public int SeekTimeInSec { get; set; }
            public int? VolumeLevel { get; set; }
            public int? BassEq { get; set; }
            public Uri Image { get; set; }
        }

        public class MyMediaSummaryDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public int SeekTimeInSec { get; set; }
            public Uri Image { get; set; }
            public int VolumeLevel { get; set; }
            public int? BassEq { get; set; }
        }

        public class MyComplexLinkedSource
        {
            public MyComplexModel Model { get; set; }
            public MyMediaLinkedSource Media { get; set; }
        }

        public class MyComplexModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public int? MediaId { get; set; }
        }

        public class MyComplexDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public MyMediaSummaryDto Media { get; set; }
        }
    }
}