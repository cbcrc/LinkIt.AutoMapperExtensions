using AutoMapper;
using NUnit.Framework;

namespace RC.AutoMapper.Tests
{
    [TestFixture]
    public class AutoMapReference_ComplexTypeAndNestingTests {

        [SetUp]
        public void SetUp()
        {
            Mapper.Reset();
            Mapper
                .CreateMap<NestedLinkedSource, MyComplexDto>()
                .MapLinkedSource();
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

            Assert.That(actual.Id, Is.EqualTo(source.Model.Id));
            Assert.That(actual.Title, Is.EqualTo(source.Model.Title));
            Assert.That(actual.Point.X, Is.EqualTo(source.Model.Point.X));
            Assert.That(actual.Point.Y, Is.EqualTo(source.Model.Point.Y));
            Assert.That(actual.Child, Is.Null);
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

            Assert.That(actual.Id, Is.EqualTo(source.Model.Id));
            Assert.That(actual.Title, Is.EqualTo(source.Model.Title));
            Assert.That(actual.Point.X, Is.EqualTo(source.Model.Point.X));
            Assert.That(actual.Point.Y, Is.EqualTo(source.Model.Point.Y));

            Assert.That(actual.Child.Id, Is.EqualTo(source.Child.Model.Id));
            Assert.That(actual.Child.Title, Is.EqualTo(source.Child.Model.Title));
            Assert.That(actual.Child.Point.X, Is.EqualTo(source.Child.Model.Point.X));
            Assert.That(actual.Child.Point.Y, Is.EqualTo(source.Child.Model.Point.Y));

            Assert.That(actual.Child.Child.Id, Is.EqualTo(source.Child.Child.Model.Id));
            Assert.That(actual.Child.Child.Title, Is.EqualTo(source.Child.Child.Model.Title));
            Assert.That(actual.Child.Child.Point.X, Is.EqualTo(source.Child.Child.Model.Point.X));
            Assert.That(actual.Child.Child.Point.Y, Is.EqualTo(source.Child.Child.Model.Point.Y));
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

            Assert.That(actual[0].Id, Is.EqualTo(source[0].Model.Id));
            Assert.That(actual[0].Title, Is.EqualTo(source[0].Model.Title));
            Assert.That(actual[0].Point.X, Is.EqualTo(source[0].Model.Point.X));
            Assert.That(actual[0].Point.Y, Is.EqualTo(source[0].Model.Point.Y));
            Assert.That(actual[0].Child, Is.Null);

            Assert.That(actual[1].Id, Is.EqualTo(source[1].Model.Id));
            Assert.That(actual[1].Title, Is.EqualTo(source[1].Model.Title));
            Assert.That(actual[1].Point.X, Is.EqualTo(source[1].Model.Point.X));
            Assert.That(actual[1].Point.Y, Is.EqualTo(source[1].Model.Point.Y));
            Assert.That(actual[1].Child, Is.Null);
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

            Assert.That(actual.Items[0].Id, Is.EqualTo(source.Items[0].Model.Id));
            Assert.That(actual.Items[0].Title, Is.EqualTo(source.Items[0].Model.Title));
            Assert.That(actual.Items[0].Point.X, Is.EqualTo(source.Items[0].Model.Point.X));
            Assert.That(actual.Items[0].Point.Y, Is.EqualTo(source.Items[0].Model.Point.Y));
            Assert.That(actual.Items[0].Child, Is.Null);

            Assert.That(actual.Items[1].Id, Is.EqualTo(source.Items[1].Model.Id));
            Assert.That(actual.Items[1].Title, Is.EqualTo(source.Items[1].Model.Title));
            Assert.That(actual.Items[1].Point.X, Is.EqualTo(source.Items[1].Model.Point.X));
            Assert.That(actual.Items[1].Point.Y, Is.EqualTo(source.Items[1].Model.Point.Y));
            Assert.That(actual.Items[1].Child, Is.Null);
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

        public class NestedLinkedSource
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