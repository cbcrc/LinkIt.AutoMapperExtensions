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
    public class AutoMapReferencePerformanceTests {
        [Test]
        public void PerformanceImpact_Manual()
        {
            Mapper
                .CreateMap<LinkedSource, ADto>()
                .ForMember(dto => dto.X, o => o.MapFrom(linkedSource => linkedSource.Model.X))
                .ForMember(dto => dto.Y, o => o.MapFrom(linkedSource => linkedSource.Model.Y));
            Mapper.CreateMap<AReference, AReferenceDto>();

            Mapper.AssertConfigurationIsValid();

            var source = CreateSource();
            Benchmark(source);
        }

        private static void Benchmark(LinkedSource source)
        {
            var actual = Mapper.Map<ADto>(source);
            AssertDestination(actual);

            var durations = new List<long>();
            for (int times = 0; times < 1000; times++)
            {
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < 1000; i++)
                {
                    actual = Mapper.Map<ADto>(source);
                }
                watch.Stop();
                durations.Add(watch.ElapsedMilliseconds);
            }

            Console.WriteLine(durations.Average() + " ms");
        }

        [Test]
        public void PerformanceImpact_Auto() {
            Mapper
                .CreateMap<LinkedSource, ADto>()
                .MapModel<LinkedSource, AModel, ADto>();
            Mapper.CreateMap<AReference, AReferenceDto>();

            Mapper.AssertConfigurationIsValid();

            var source = CreateSource();
            Benchmark(source);
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