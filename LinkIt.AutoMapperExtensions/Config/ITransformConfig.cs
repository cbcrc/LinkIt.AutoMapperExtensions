#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using AutoMapper;

namespace LinkIt.AutoMapperExtensions.Config
{
    public interface ITransformConfig
    {
        void ConfigureTransformation(IMapperConfigurationExpression config);
    }
}