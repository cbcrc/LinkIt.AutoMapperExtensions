using AutoMapper;
using NUnit.Framework;

namespace RC.AutoMapper.Tests
{
    [TestFixture]
    public class AutoMapReference_ContextualizationTests
    {
        [SetUp]
        public void SetUp()
        {
            Mapper.CreateMap<MyMediaLinkedSource, MyMediaSummaryDto>()
                .MapLinkedSource()
                .ForMember(dto => dto.Id, member => member.MapFrom(source => source.Model.Id))
                .ForMember(dto => dto.SeekTimeInSec, member => member.MapFrom(source => source.ModelContextualization.SeekTimeInSec));
            Mapper.CreateMap<MyComplexLinkedSource, MyComplexDto>()
                .MapLinkedSource();
        }

        [TearDown]
        public void TearDown()
        {
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
                ModelContextualization = new MyMediaContextualization
                {
                    Id = 1,
                    SeekTimeInSec = 32,
                    Title = "Overridden title"
                }
            };

            var actual = Mapper.Map<MyMediaSummaryDto>(linkedSource);

            Assert.That(actual.Title, Is.EqualTo("Overridden title"));
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
                ModelContextualization = new MyMediaContextualization
                {
                    Id = 1,
                    SeekTimeInSec = 32,
                    Title = null
                }
            };

            var actual = Mapper.Map<MyMediaSummaryDto>(linkedSource);

            Assert.That(actual.Title, Is.EqualTo("The title"));
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
                ModelContextualization = new MyMediaContextualization
                {
                    Id = 1,
                    SeekTimeInSec = 32,
                    Title = "Overridden title"
                }
            };

            var actual = Mapper.Map<MyMediaSummaryDto>(linkedSource);

            Assert.That(actual.SeekTimeInSec, Is.EqualTo(32));
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
                    ModelContextualization = new MyMediaContextualization()
                    {
                        Id = 1,
                        SeekTimeInSec = 32,
                        Title = "Overridden title"
                    }
                }
            };

            var actual = Mapper.Map<MyComplexDto>(linkedSource);

            Assert.That(actual.Media.Title, Is.EqualTo("Overridden title"));
        }


        public class MyMediaLinkedSource
        {
            public MyMedia Model { get; set; }
            public MyMediaContextualization ModelContextualization { get; set; }
        }

        public class MyMedia
        {
            public int Id { get; set; }
            public string Title { get; set; }
        }

        public class MyMediaContextualization
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public int SeekTimeInSec { get; set; }
        }

        public class MyMediaSummaryDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public int SeekTimeInSec { get; set; }
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