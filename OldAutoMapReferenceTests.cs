using System;
using System.Linq;
using System.Linq.Expressions;
using ApprovalTests.Reporters;
using AutoMapper;
using NUnit.Framework;

namespace ShowMeAnExampleOfAutomapperFromLinkedSource {
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class OldAutoMapReferenceTests {
        [Test]
        public void Map_OneStep()
        {
            //todo: write a dynamic mapping expression for model

            Mapper.CreateMap<LinkedSource, ADto>();
            Mapper.CreateMap<AModel, ADto>().ForMember(dest => dest.Reference, opt => opt.Ignore());

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
            Mapper.CreateMap<AModel, ADto>().ForMember(dest => dest.Reference, opt => opt.Ignore());

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
            Assert.That(actual.Reference, Is.Not.Null);
            Assert.That(actual.Reference.A, Is.EqualTo("TheA"));
            Assert.That(actual.Reference.B, Is.EqualTo("TheB"));

            Assert.That(actual.X, Is.EqualTo("TheX"));
            Assert.That(actual.Y, Is.EqualTo("TheY"));
        }
    }
}
