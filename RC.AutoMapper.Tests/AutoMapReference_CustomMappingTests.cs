using AutoMapper;
using NUnit.Framework;

namespace RC.AutoMapper.Tests
{
    [TestFixture]
    public class AutoMapReference_CustomMappingTests
    {
        [SetUp]
        public void SetUp()
        {
            Mapper.Reset();
        }

        [Test]
        public void MapModel_WithCustomMappingForOtherProperty_ShouldMap()
        {
            Mapper
                .CreateMap<MyCustomLinkedSource, MyCustomDto>()
                .MapLinkedSource()
                .ForMember(destination => destination.SelfUrl, opt => opt.MapFrom(src => "http://blah.com/" + src.Model.Id));

            Mapper.AssertConfigurationIsValid();

            var source = new MyCustomLinkedSource
            {
                Model = CreateMyCustomModel(1),
            };
                
            var actual = Mapper.Map<MyCustomDto>(source);

            Assert.That(actual.SelfUrl, Is.EqualTo("http://blah.com/" + source.Model.Id));
        }

        [Test]
        public void MapModel_WithCustomMappingForModelProperty_ShouldMap()
        {
            Mapper
                .CreateMap<MyCustomLinkedSource, MyCustomDto>()
                .MapLinkedSource()
                .ForMember(destination => destination.Title, opt => opt.MapFrom(src => src.Model.Title + " TEST"))
                .ForMember(destination => destination.SelfUrl, opt => opt.Ignore());

            Mapper.AssertConfigurationIsValid();

            var source = new MyCustomLinkedSource
            {
                Model = CreateMyCustomModel(1),
            };

            var actual = Mapper.Map<MyCustomDto>(source);

            Assert.That(actual.Title, Is.EqualTo(source.Model.Title + " TEST"));
        }

        private static MyCustomModel CreateMyCustomModel(int id)
        {
            return new MyCustomModel{
                Id = id,
                Title = "Title "+id,
            };
        }

        public class MyCustomLinkedSource
        {
            public MyCustomModel Model { get; set; }
        }

        public class MyCustomModel {
            public int Id { get; set; }
            public string Title { get; set; }
        }

        public class MyCustomDto {
            public string SelfUrl { get; set; }
            public string Title { get; set; }
        }


    }
}