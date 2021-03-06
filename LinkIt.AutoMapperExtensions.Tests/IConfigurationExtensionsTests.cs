﻿#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using System;
using AutoMapper;
using LinkIt.AutoMapperExtensions.Config;
using Xunit;

namespace LinkIt.AutoMapperExtensions.Tests {
    public class IConfigurationExtensionsTests {
        [Fact]
        public void Initialize_WithAbstractTransformConfig_ShouldInitializedAbstractTransformConfigFirst() {
            Mapper.Initialize(
                configuration => configuration.ApplyTransformConfigs(
                    new[] { typeof(IConfigurationExtensionsTests).Assembly }
                )
            );

            Mapper.AssertConfigurationIsValid();

            //Two tests in one method because Mapper initialization is static
            var source = new Model{
                NotByConvention = new Uri("http://from-model.com/index.html")
            };
            var actual = Mapper.Map<Dto>(source);

            Assert.Equal("http://from-interface.com/index.html", actual.FromInterface.ToString());
            Assert.Equal("http://from-model.com/index.html", actual.FromModel.ToString());
        }

        public interface IDto
        {
            Uri FromInterface { get; set; }
        }
        public class Dto : IDto
        {
            public Uri FromInterface { get; set; }
            public Uri FromModel { get; set; }
        }

        public interface IModel
        {
        }

        public class Model:IModel
        {
            public Uri NotByConvention { get; set; }
        }

        public class BBeforeA_IInterfaceConfig : IAbstractTransformConfig
        {
            public void ConfigureTransformation(IMapperConfigurationExpression config)
            {
                config.CreateMap<IModel, IDto>()
                    .Include<Model, Dto>()
                    .ForMember(
                            dto => dto.FromInterface,
                            member => member.UseValue(new Uri("http://from-interface.com/index.html"))
                    );
            }
        }

        public class ABeforeB_ModelConfig : ITransformConfig {
            public void ConfigureTransformation(IMapperConfigurationExpression config) {
                config.CreateMap<Model, Dto>()
                    .ForMember(
                            dto => dto.FromModel,
                            member => member.MapFrom(source=>source.NotByConvention)
                    );

            }
        }


    }
}
