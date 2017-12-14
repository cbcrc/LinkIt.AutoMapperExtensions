#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using AutoMapper;
using Xunit;

namespace LinkIt.AutoMapperExtensions.Tests
{
    public class AutoMapReference_ComplexTypeAndNestingTests
    {
        private MapperConfiguration _config;
        private IMapper _mapper;
        
        public AutoMapReference_ComplexTypeAndNestingTests()
        {
            _config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<NestedLinkedSource, MyComplexDto>()
                    .MapLinkedSource();
                cfg.CreateMap<MyPoint, MyPointDto>();
                cfg.CreateMap<ListOfNestedLinkedSource, ListOfMyComplexDto>();
            });
            _mapper = _config.CreateMapper();
        }

        [Fact]
        public void AssertConfigurationIsValid()
        {
            _config.AssertConfigurationIsValid();
        }

        [Fact]
        public void MapModelOnly_WithIntAndComplexType_ShouldMap()
        {
            var source = new NestedLinkedSource
            {
                Model = CreateMyComplexModel(1, null),
                Child = null
            };
                
            var actual = _mapper.Map<MyComplexDto>(source);

            Assert.Equal(source.Model.Id, actual.Id);
            Assert.Equal(source.Model.Title, actual.Title);
            Assert.Equal(source.Model.Point.X, actual.Point.X);
            Assert.Equal(source.Model.Point.Y, actual.Point.Y);
            Assert.Null(actual.Child);
        }

        [Fact]
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

            var actual = _mapper.Map<MyComplexDto>(source);

            Assert.Equal(source.Model.Id, actual.Id);
            Assert.Equal(source.Model.Title, actual.Title);
            Assert.Equal(source.Model.Point.X, actual.Point.X);
            Assert.Equal(source.Model.Point.Y, actual.Point.Y);

            Assert.Equal(source.Child.Model.Id, actual.Child.Id);
            Assert.Equal(source.Child.Model.Title, actual.Child.Title);
            Assert.Equal(source.Child.Model.Point.X, actual.Child.Point.X);
            Assert.Equal(source.Child.Model.Point.Y, actual.Child.Point.Y);

            Assert.Equal(source.Child.Child.Model.Id, actual.Child.Child.Id);
            Assert.Equal(source.Child.Child.Model.Title, actual.Child.Child.Title);
            Assert.Equal(source.Child.Child.Model.Point.X, actual.Child.Child.Point.X);
            Assert.Equal(source.Child.Child.Model.Point.Y, actual.Child.Child.Point.Y);
        }

        [Fact]
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

            var actual = _mapper.Map<MyComplexDto[]>(source);

            Assert.Equal(source[0].Model.Id, actual[0].Id);
            Assert.Equal(source[0].Model.Title, actual[0].Title);
            Assert.Equal(source[0].Model.Point.X, actual[0].Point.X);
            Assert.Equal(source[0].Model.Point.Y, actual[0].Point.Y);
            Assert.Null(actual[0].Child);

            Assert.Equal(source[1].Model.Id, actual[1].Id);
            Assert.Equal(source[1].Model.Title, actual[1].Title);
            Assert.Equal(source[1].Model.Point.X, actual[1].Point.X);
            Assert.Equal(source[1].Model.Point.Y, actual[1].Point.Y);
            Assert.Null(actual[1].Child);
        }

        [Fact]
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

            var actual = _mapper.Map<ListOfMyComplexDto>(source);

            Assert.Equal(source.Items[0].Model.Id, actual.Items[0].Id);
            Assert.Equal(source.Items[0].Model.Title, actual.Items[0].Title);
            Assert.Equal(source.Items[0].Model.Point.X, actual.Items[0].Point.X);
            Assert.Equal(source.Items[0].Model.Point.Y, actual.Items[0].Point.Y);
            Assert.Null(actual.Items[0].Child);

            Assert.Equal(source.Items[1].Model.Id, actual.Items[1].Id);
            Assert.Equal(source.Items[1].Model.Title, actual.Items[1].Title);
            Assert.Equal(source.Items[1].Model.Point.X, actual.Items[1].Point.X);
            Assert.Equal(source.Items[1].Model.Point.Y, actual.Items[1].Point.Y);
            Assert.Null(actual.Items[1].Child);
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