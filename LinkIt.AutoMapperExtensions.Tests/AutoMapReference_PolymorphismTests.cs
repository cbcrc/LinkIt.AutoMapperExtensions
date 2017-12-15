#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using AutoMapper;
using Xunit;

namespace LinkIt.AutoMapperExtensions.Tests
{
    public class AutoMapReference_PolymorphismTests
    {
        private readonly MapperConfiguration _config;
        private readonly IMapper _mapper;
        
        public AutoMapReference_PolymorphismTests()
        {
            _config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PolymorphicModel, PolymorphicDto>();

                cfg.CreateMap<PolymorphicValueA, PolymorphicValueADto>();
                cfg.CreateMap<PolymorphicValueB, PolymorphicValueBDto>();
                cfg.CreateMap<PolymorphicValueC, PolymorphicValueCDto>()
                    .ForMember(dto => dto.Y, member => member.ResolveUsing(source => source.Y + "-custom"));


                cfg.CreateMap<IPolymorphicValue, IPolymorphicValueDto>()
                    .Include<PolymorphicValueA, PolymorphicValueADto>()
                    .Include<PolymorphicValueB, PolymorphicValueBDto>()
                    .Include<PolymorphicValueC, PolymorphicValueCDto>();
            });
            _mapper = _config.CreateMapper();
        }

        [Fact]
        public void AssertConfigurationIsValid()
        {
            _config.AssertConfigurationIsValid();
        }


        [Fact]
        public void Map_WithPolymorphicValueA_ShouldMapPolymorphicValueADto()
        {
            var source = new PolymorphicModel
            {
                MyValue = new PolymorphicValueA
                {
                    X = "TheX",
                    Y = "TheY",
                }
            };

            var actual = _mapper.Map<PolymorphicDto>(source);

            var asPolymorphicValueADto = actual.MyValue as PolymorphicValueADto;
            Assert.NotNull(asPolymorphicValueADto);
            Assert.Equal("TheX", asPolymorphicValueADto.X);
            Assert.Equal("TheY", asPolymorphicValueADto.Y);
        }

        [Fact]
        public void Map_WithPolymorphicValueB_ShouldMapPolymorphicValueBDto() {
            var source = new PolymorphicModel {
                MyValue = new PolymorphicValueB {
                    Y = "TheY",
                    Z = "TheZ",
                }
            };

            var actual = _mapper.Map<PolymorphicDto>(source);

            var asPolymorphicValueBDto = actual.MyValue as PolymorphicValueBDto;
            Assert.NotNull(asPolymorphicValueBDto);
            Assert.Equal("TheY", asPolymorphicValueBDto.Y);
            Assert.Equal("TheZ", asPolymorphicValueBDto.Z);
        }

        [Fact]
        public void Map_WithCustomRules() {
            var source = new PolymorphicModel {
                MyValue = new PolymorphicValueC {
                    Y = "TheY",
                }
            };

            var actual = _mapper.Map<PolymorphicDto>(source);

            var asPolymorphicValueCDto = actual.MyValue as PolymorphicValueCDto;
            Assert.NotNull(asPolymorphicValueCDto);
            Assert.Equal("TheY-custom", asPolymorphicValueCDto.Y);
        }
        

        public class PolymorphicModel {
            public IPolymorphicValue MyValue { get; set; }
        }

        public interface IPolymorphicValue {
            string Y { get; set; }
        }

        public class PolymorphicValueA : IPolymorphicValue {
            public string X { get; set; }
            public string Y { get; set; }
        }

        public class PolymorphicValueB : IPolymorphicValue {
            public string Y { get; set; }
            public string Z { get; set; }
        }

        public class PolymorphicValueC : IPolymorphicValue {
            public string Y { get; set; }
        }



        public interface PolymorphicDto {
            IPolymorphicValueDto MyValue { get; set; }
        }

        public interface IPolymorphicValueDto {
            string Y { get; set; }
        }

        public class PolymorphicValueADto : IPolymorphicValueDto {
            public string X { get; set; }
            public string Y { get; set; }
       }

        public class PolymorphicValueBDto : IPolymorphicValueDto {
            public string Y { get; set; }
            public string Z { get; set; }
        }

        public class PolymorphicValueCDto : IPolymorphicValueDto {
            public string Y { get; set; }
        }

    }
}