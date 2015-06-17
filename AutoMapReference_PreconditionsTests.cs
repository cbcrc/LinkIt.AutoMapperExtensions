using System;
using System.Drawing;
using ApprovalTests.Reporters;
using AutoMapper;
using NUnit.Framework;
using RC.Testing;

namespace ShowMeAnExampleOfAutomapperFromLinkedSource
{
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class AutoMapReference_PreconditionsTests {

        [Test]
        public void MapModel_WithoutModelProperty_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(()=>            
                Mapper
                .CreateMap<WannaBeLinkedSource, string>()
                .MapModel()
            );
        }

        [Test]
        public void MapModel_WithPrimitiveModel_ShouldThrowArgumentException() {
            Assert.Throws<ArgumentException>(() =>
                Mapper
                .CreateMap<PrimitiveLinkedSource, string>()
                .MapModel()
            );
        }

        [Test]
        public void MapModel_WithArrayModel_ShouldThrowArgumentException() {
            Assert.Throws<ArgumentException>(() =>
                Mapper
                .CreateMap<ArrayLinkedSource, string>()
                .MapModel()
            );
        }



        public class WannaBeLinkedSource
        {
            public Point NotAModel { get; set; }
        }

        public class PrimitiveLinkedSource {
            public int Model { get; set; }
        }

        public class ArrayLinkedSource {
            public Point[] Model { get; set; }
        }



    }
}