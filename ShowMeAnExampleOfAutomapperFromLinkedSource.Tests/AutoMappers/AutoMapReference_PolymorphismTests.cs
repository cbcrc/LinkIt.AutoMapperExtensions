using ApprovalTests.Reporters;
using AutoMapper;
using NUnit.Framework;
using RC.Testing;
using ShowMeAnExampleOfAutomapperFromLinkedSource.AutoMappers;

namespace ShowMeAnExampleOfAutomapperFromLinkedSource.Tests.AutoMappers
{
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class AutoMapReference_PolymorphismTests
    {
        [Test]
        public void AssertConfigurationIsValid()
        {
            CreateMappings();
            Mapper.AssertConfigurationIsValid();
        }
        

        [Test]
        public void Map_WithPolymorphicValueA_ShouldMapPolymorphicValueADto()
        {
            var source = new PolymorphicModel
            {
                MyValue = new PolymorphicValueA
                {
                    X = "TheX",
                    Y = "TheY",
                }
            };
            CreateMappings();

            var actual = Mapper.Map<PolymorphicDto>(source);

            var asPolymorphicValueADto = actual.MyValue as PolymorphicValueADto;
            Assert.That(asPolymorphicValueADto, Is.Not.Null);
            Assert.That(asPolymorphicValueADto.X, Is.EqualTo("TheX"));
            Assert.That(asPolymorphicValueADto.Y, Is.EqualTo("TheY"));
        }

        [Test]
        public void Map_WithPolymorphicValueB_ShouldMapPolymorphicValueBDto() {
            var source = new PolymorphicModel {
                MyValue = new PolymorphicValueB {
                    Y = "TheY",
                    Z = "TheZ",
                }
            };
            CreateMappings();

            var actual = Mapper.Map<PolymorphicDto>(source);

            var asPolymorphicValueBDto = actual.MyValue as PolymorphicValueBDto;
            Assert.That(asPolymorphicValueBDto, Is.Not.Null);
            Assert.That(asPolymorphicValueBDto.Y, Is.EqualTo("TheY"));
            Assert.That(asPolymorphicValueBDto.Z, Is.EqualTo("TheZ"));
        }

        private static void CreateMappings()
        {
            Mapper.CreateMap<PolymorphicModel, PolymorphicDto>();
            Mapper.CreateMap<IPolymorphicValue, IPolymorphicValueDto>()
                .Include<PolymorphicValueA, PolymorphicValueADto>()
                .Include<PolymorphicValueB, PolymorphicValueBDto>();
        }


        

        public class PolymorphicModel {
            public IPolymorphicValue MyValue { get; set; }
        }

        public interface IPolymorphicValue {
            string Y { get; set; }
        }

        public class PolymorphicValueA : IPolymorphicValue {
            public string X { get; set; }
            public string Y { get; set; }
        }

        public class PolymorphicValueB : IPolymorphicValue {
            public string Y { get; set; }
            public string Z { get; set; }
        }


        public interface PolymorphicDto {
            IPolymorphicValueDto MyValue { get; set; }
        }

        public interface IPolymorphicValueDto {
            string Y { get; set; }
        }

        public class PolymorphicValueADto : IPolymorphicValueDto {
            public string X { get; set; }
            public string Y { get; set; }
       }

        public class PolymorphicValueBDto : IPolymorphicValueDto {
            public string Y { get; set; }
            public string Z { get; set; }
        }

    }
}