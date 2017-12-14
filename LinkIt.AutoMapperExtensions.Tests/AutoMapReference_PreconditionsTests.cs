#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using System;
using AutoMapper;
using Xunit;

namespace LinkIt.AutoMapperExtensions.Tests
{
    public class AutoMapReference_PreconditionsTests 
    {
        [Fact]
        public void MapModel_WithoutModelProperty_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<WannaBeLinkedSource, string>()
                        .MapLinkedSource();
                });
            });
        }

        [Fact]
        public void MapModel_WithPrimitiveModel_ShouldThrowArgumentException() {
            Assert.Throws<ArgumentException>(() =>
            {
                new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<PrimitiveLinkedSource, string>()
                        .MapLinkedSource();
                });
            });
        }


        public class WannaBeLinkedSource
        {
            public Tuple<string, string> NotAModel { get; set; }
        }

        public class PrimitiveLinkedSource {
            public int Model { get; set; }
        }
    }
}