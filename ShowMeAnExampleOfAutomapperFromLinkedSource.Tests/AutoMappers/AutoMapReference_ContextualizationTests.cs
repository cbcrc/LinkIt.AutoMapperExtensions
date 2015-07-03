using ApprovalTests.Reporters;
using AutoMapper;
using NUnit.Framework;

namespace ShowMeAnExampleOfAutomapperFromLinkedSource.Tests.AutoMappers
{
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class AutoMapReference_ContextualizationTests
    {
        [SetUp]
        public void SetUp() {
            Mapper.CreateMap<MyMediaLinkedSource, MyMediaSummaryDto>()
                .ForMember(dto => dto.Id, member => member.MapFrom(source => source.Model.Id))
                .ForMember(dto => dto.Title, member => member.MapFrom(source => source.ModelContextualization.Title ?? source.Model.Title))
                .ForMember(dto => dto.SeekTimeInSec, member => member.MapFrom(source => source.ModelContextualization.SeekTimeInSec));
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
                Model = new MyMedia{
                    Id = 1,
                    Title = "The title"
                },
                ModelContextualization = new MyMediaContextualization()
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
        public void Map_WithAdditionInContextualization_ShouldMapAddition() {
            var linkedSource = new MyMediaLinkedSource {
                Model = new MyMedia {
                    Id = 1,
                    Title = "The title"
                },
                ModelContextualization = new MyMediaContextualization() {
                    Id = 1,
                    SeekTimeInSec = 32,
                    Title = "Overridden title"
                }
            };

            var actual = Mapper.Map<MyMediaSummaryDto>(linkedSource);

            Assert.That(actual.SeekTimeInSec, Is.EqualTo(32));
        }


        public class MyMediaLinkedSource
        {
            public MyMedia Model { get; set; }
            public MyMediaContextualization ModelContextualization { get; set; }
        }

        public class MyMediaContextualization {
            public int Id { get; set; }
            public string Title { get; set; }
            public int SeekTimeInSec { get; set; }
//            public string OveridingSummaryImageUrl { get; set; }
        }

        public class MyCustomDto {
            public string SelfUrl { get; set; }
            public string Title { get; set; }
        }

        public class MyMediaSummaryDto 
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public int SeekTimeInSec { get; set; }
        }

        public class MyMedia {
            public int Id { get; set; }
            public string Title { get; set; }
            //        public string SummaryImageUrl { get; set; }
        }

    }
}