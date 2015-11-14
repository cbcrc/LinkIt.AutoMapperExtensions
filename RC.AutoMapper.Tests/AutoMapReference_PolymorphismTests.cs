using AutoMapper;
using NUnit.Framework;

namespace RC.AutoMapper.Tests
{
    [TestFixture]
    public class AutoMapReference_PolymorphismTests
    {
        [SetUp]
        public void SetUp()
        {
            Mapper.CreateMap<PolymorphicModel, PolymorphicDto>();

            Mapper.CreateMap<PolymorphicValueA, PolymorphicValueADto>();
            Mapper.CreateMap<PolymorphicValueB, PolymorphicValueBDto>();
            Mapper.CreateMap<PolymorphicValueC, PolymorphicValueCDto>()
                .ForMember(dto => dto.Y, member => member.ResolveUsing(source => source.Y + "-custom"));


            Mapper.CreateMap<IPolymorphicValue, IPolymorphicValueDto>()
                .Include<PolymorphicValueA, PolymorphicValueADto>()
                .Include<PolymorphicValueB, PolymorphicValueBDto>()
                .Include<PolymorphicValueC, PolymorphicValueCDto>();

            Mapper.Configuration.Seal();

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

            var actual = Mapper.Map<PolymorphicDto>(source);

            var asPolymorphicValueBDto = actual.MyValue as PolymorphicValueBDto;
            Assert.That(asPolymorphicValueBDto, Is.Not.Null);
            Assert.That(asPolymorphicValueBDto.Y, Is.EqualTo("TheY"));
            Assert.That(asPolymorphicValueBDto.Z, Is.EqualTo("TheZ"));
        }

        [Test]
        public void Map_WithCustomRules() {
            var source = new PolymorphicModel {
                MyValue = new PolymorphicValueC {
                    Y = "TheY",
                }
            };

            var actual = Mapper.Map<PolymorphicDto>(source);

            var asPolymorphicValueCDto = actual.MyValue as PolymorphicValueCDto;
            Assert.That(asPolymorphicValueCDto, Is.Not.Null);
            Assert.That(asPolymorphicValueCDto.Y, Is.EqualTo("TheY-custom"));
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

        public class PolymorphicValueC : IPolymorphicValue {
            public string Y { get; set; }
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

        public class PolymorphicValueCDto : IPolymorphicValueDto {
            public string Y { get; set; }
        }

    }
}