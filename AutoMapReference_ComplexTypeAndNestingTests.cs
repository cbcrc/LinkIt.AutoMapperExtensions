using ApprovalTests.Reporters;
using AutoMapper;
using NUnit.Framework;
using RC.Testing;

namespace ShowMeAnExampleOfAutomapperFromLinkedSource
{
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class AutoMapReference_ComplexTypeAndNestingTests {

        [SetUp]
        public void SetUp()
        {
            Mapper
                .CreateMap<NestedLinkedSource, MyComplexDto>()
                .MapModel<NestedLinkedSource, MyComplexModel, MyComplexDto>();
            Mapper.CreateMap<MyPoint, MyPointDto>();
            Mapper.CreateMap<ListOfNestedLinkedSource, ListOfMyComplexDto>();
        }

        [Test]
        public void AssertConfigurationIsValid()
        {
            Mapper.AssertConfigurationIsValid();
        }

        [Test]
        public void MapModelOnly_WithIntAndComplexType_ShouldMap()
        {
            var source = new NestedLinkedSource
            {
                Model = CreateMyComplexModel(1, null),
                Child = null
            };
                
            var actual = Mapper.Map<MyComplexDto>(source);

            ApprovalsExt.VerifyPublicProperties(actual);
        }

        [Test]
        public void MapModelOnly_TwoNestedLevel_ShouldMap() {
            var source = new NestedLinkedSource {
                Model = CreateMyComplexModel(1, 2),
                Child = new NestedLinkedSource {
                    Model = CreateMyComplexModel(2, 3),
                    Child = new NestedLinkedSource {
                        Model = CreateMyComplexModel(3, null),
                        Child = null
                    }
                }
            };

            var actual = Mapper.Map<MyComplexDto>(source);

            ApprovalsExt.VerifyPublicProperties(actual);
        }

        [Test]
        public void MapModelOnly_ArrayOfLinkedSourceAtRoot_ShouldMap() {
            var source = new []
            {
                new NestedLinkedSource {
                    Model = CreateMyComplexModel(1, null),
                    Child = null
                },
                new NestedLinkedSource {
                    Model = CreateMyComplexModel(2, null),
                    Child = null
                }                
            };

            var actual = Mapper.Map<MyComplexDto[]>(source);

            ApprovalsExt.VerifyPublicProperties(actual);
        }

        [Test]
        public void MapModelOnly_ArrayOfLinkedSourceAsReference_ShouldMap() {
            var source = new ListOfNestedLinkedSource
            {
                Items = new []{
                    new NestedLinkedSource {
                        Model = CreateMyComplexModel(1, null),
                        Child = null
                    },
                    new NestedLinkedSource {
                        Model = CreateMyComplexModel(2, null),
                        Child = null
                    }              
                }
            };

            var actual = Mapper.Map<ListOfMyComplexDto>(source);

            ApprovalsExt.VerifyPublicProperties(actual);
        }


        private static MyComplexModel CreateMyComplexModel(int id, int? childId)
        {
            return new MyComplexModel{
                Id = id,
                Title = "Title "+id,
                Point = new MyPoint {X = id, Y = id},
                ChildId = childId
            };
        }

        public class NestedLinkedSource:ILinkedSource<MyComplexModel>
        {
            public MyComplexModel Model { get; set; }
            public NestedLinkedSource Child { get; set; }
        }

        public class MyComplexModel {
            public int Id { get; set; }
            public string Title { get; set; }
            public MyPoint Point { get; set; }
            public int? ChildId { get; set; }
        }

        public class MyPoint {
            public float X { get; set; }
            public float Y { get; set; }
        }

        public class MyComplexDto {
            public int Id { get; set; }
            public string Title { get; set; }
            public MyPointDto Point { get; set; }
            public MyComplexDto Child { get; set; }
        }

        public class MyPointDto {
            public float X { get; set; }
            public float Y { get; set; }
        }


        public class ListOfNestedLinkedSource {
            public NestedLinkedSource[] Items { get; set; }
        }

        public class ListOfMyComplexDto {
            public MyComplexDto[] Items { get; set; }
        }


    }
}