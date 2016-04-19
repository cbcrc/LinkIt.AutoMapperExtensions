#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using AutoMapper;

namespace LinkIt.AutoMapperExtensions.Config
{
    //!!! WARNING: Inherited mappings are error-prone !!!
    //
    //The problem is with 
    //1)Inherited Explicit Mapping having a higher priority than Convention Mapping
    //  For example, if a default is configured at the abstract level, 
    //  it is easy to forget to map the property at the implementation level.
    //2)Inherited Ignore Property are error-prone: 
    //  ignore property at the abstract type level should not be inherited, because it easy to 
    //  forget to map the property at the implementation level
    //
    //Mapping priorities from the highest to the lowest priority
    //- Explicit Mapping (using .MapFrom())
    //- Inherited Explicit Mapping
    //- Ignore Property Mapping
    //- Convention Mapping (Properties that are matched via convention)
    //- Inherited Ignore Property Mapping
    //Source: https://github.com/AutoMapper/AutoMapper/wiki/Mapping-inheritance
    //
    public interface IAbstractTransformConfig : ITransformConfig
    {
    }
}