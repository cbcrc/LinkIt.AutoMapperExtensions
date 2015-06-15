using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            Mapper
                .CreateMap<LinkedSource, ADto>()
                .MapModel<LinkedSource, AModel, ADto>();
            Mapper.CreateMap<AReference, AReferenceDto>();

            Mapper.AssertConfigurationIsValid();

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
    }
}