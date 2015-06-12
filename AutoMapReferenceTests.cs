using ApprovalTests.Reporters;
using AutoMapper;
using NUnit.Framework;
using RC.Testing;

namespace ShowMeAnExampleOfAutomapperFromLinkedSource
{
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class AutoMapReferenceTests {

        [Test]
        public void Map_DynamicMappingExpression_ShouldGenerateValidMapping() {
            Mapper.CreateMap<LinkedSource, ADto>().MapModel<LinkedSource, AModel, ADto>();
            //            Mapper.AssertConfigurationIsValid();

            var source = CreateSource();

            var actual = Mapper.Map<ADto>(source);

            ApprovalsExt.VerifyPublicProperties(actual);
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
}