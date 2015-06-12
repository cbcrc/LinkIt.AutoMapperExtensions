using System.Linq;
using ApprovalTests.Reporters;
using AutoMapper;
using NUnit.Framework;

namespace ShowMeAnExampleOfAutomapperFromLinkedSource {
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class AutoMapReferenceTests {


        [Test]
        public void Map_OneStep()
        {
            //todo: write a dynamic mapping expression for model

            Mapper.CreateMap<LinkedSource, ADto>();
            Mapper.CreateMap<AModel, ADto>().ForMember(dest => dest.ReferenceDto, opt => opt.Ignore());

            Mapper.CreateMap<AReference, AReferenceDto>();
//            Mapper.AssertConfigurationIsValid();

            var source = CreateSource();

            var actual = Mapper.Map<ADto>(source);

            AssertDestination(actual);
        }


        [Test]
        public void Map_OneStepWithBeforeMapConfig()//We loose the ability to AssertConfigurationIsValid
        {
            Mapper.CreateMap<LinkedSource, ADto>()
                .BeforeMap((s, d) => Mapper.Map(s.Model, d));
            Mapper.CreateMap<AModel, ADto>().ForMember(dest => dest.ReferenceDto, opt => opt.Ignore());

            Mapper.CreateMap<AReference, AReferenceDto>();

            var source = CreateSource();

            var actual = Mapper.Map<ADto>(source);

            AssertDestination(actual);
        }


        [Test]
        public void Map_TwoSteps() //We loose the ability to AssertConfigurationIsValid
        {
            Mapper.CreateMap<AModel, ADto>();
            Mapper.CreateMap<LinkedSource, ADto>();
            Mapper.CreateMap<AReference, AReferenceDto>();

            var source = CreateSource();

            var actual = Mapper.Map<ADto>(source);
            Mapper.Map(source.Model, actual);

            AssertDestination(actual);
        }

        private static LinkedSource CreateSource()
        {
            return new LinkedSource{
                Model = new AModel{
                    X = "TheX",
                    Y = "TheY",
                    Reference = 32
                },
                Reference = new AReference
                {
                    A="TheA",
                    B="TheB"
                }
            };
        }

        private static void AssertDestination(ADto actual) {
            Assert.That(actual.ReferenceDto, Is.Not.Null);
            Assert.That(actual.ReferenceDto.A, Is.EqualTo("TheA"));
            Assert.That(actual.ReferenceDto.B, Is.EqualTo("TheB"));

            Assert.That(actual.X, Is.EqualTo("TheX"));
            Assert.That(actual.Y, Is.EqualTo("TheY"));
        }
    }

    public interface ILinkedSource<T>
    {
        T Model { get; }
    }

    public class LinkedSource:ILinkedSource<AModel>
    {
        public AModel Model { get; set; }
        public AReference Reference { get; set; }
    }

    public class AModel {
        public string X { get; set; }
        public string Y { get; set; }
        public int Reference { get; set; }
    }

    public class AReference {
        public string A { get; set; }
        public string B { get; set; }
    }

    public class ADto {
        public string X { get; set; }
        public string Y { get; set; }
        public AReferenceDto ReferenceDto { get; set; }
    }

    public class AReferenceDto {
        public string A { get; set; }
        public string B { get; set; }
    }

    public static class MappingExpressionExtensions {
        //Useless since it defeat the purpose of AssertConfigurationIsValid
        //However, it is a good inspiration for dynamic mapping expression based on Model
        public static IMappingExpression<TSource, TDestination> IgnoreAllNonExisting<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression) {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);
            var existingMaps = Mapper.GetAllTypeMaps().First(x => x.SourceType.Equals(sourceType)
                && x.DestinationType.Equals(destinationType));
            foreach (var property in existingMaps.GetUnmappedPropertyNames()) {
                expression.ForMember(property, opt => opt.Ignore());
            }
            return expression;
        }
    }
}
