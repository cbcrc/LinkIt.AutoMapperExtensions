using System;
using AutoMapper;
using NUnit.Framework;

namespace RC.AutoMapper.Tests
{
    [TestFixture]
    public class AutoMapReference_ContextualizationTests
    {
        [SetUp]
        public void SetUp() {
            Mapper.CreateMap<MyMediaLinkedSource, MyMediaSummaryDto>()
                .MapLinkedSource()
                .ForMember(dto => dto.Id, member => member.MapFrom(source => source.Model.Id));
            Mapper.CreateMap<MyComplexLinkedSource, MyComplexDto>()
                .MapLinkedSource();
        }

        [TearDown]
        public void TearDown() {
            Mapper.Reset();
        }

        [Test]
        public void AssertConfigurationIsValid()
        {
            Mapper.AssertConfigurationIsValid();
        }


        [Test]
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

            var actual = Mapper.Map<MyMediaSummaryDto>(linkedSource);

            Assert.That(actual.Title, Is.EqualTo(linkedSource.Contextualization.Title));
        }

        [Test]
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

            var actual = Mapper.Map<MyMediaSummaryDto>(linkedSource);

            Assert.That(actual.Title, Is.EqualTo(linkedSource.Model.Title));
        }

        [Test]
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

            var actual = Mapper.Map<MyMediaSummaryDto>(linkedSource);

            Assert.That(actual.SeekTimeInSec, Is.EqualTo(linkedSource.Contextualization.SeekTimeInSec));
        }

        [Test]
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

            var actual = Mapper.Map<MyMediaSummaryDto>(linkedSource);

            Assert.That(actual.Title, Is.EqualTo(linkedSource.Model.Title));
        }


        [Test]
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

            var actual = Mapper.Map<MyComplexDto>(linkedSource);

            Assert.That(actual.Media.Title, Is.EqualTo(linkedSource.Media.Contextualization.Title));
        }



        [Test]
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

            var actual = Mapper.Map<MyMediaSummaryDto>(linkedSource);

            Assert.That(actual.VolumeLevel, Is.EqualTo(0));
            Assert.That(actual.BassEq.Value, Is.EqualTo(5));
        }

        [Test]
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

            var actual = Mapper.Map<MyMediaSummaryDto>(linkedSource);

            Assert.That(actual.VolumeLevel, Is.EqualTo(1));
            Assert.That(actual.BassEq, Is.EqualTo(2));
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