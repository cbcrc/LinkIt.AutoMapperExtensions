#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using AutoMapper;

namespace LinkIt.AutoMapperExtensions
{
    public static class MappingExpressionExtensions
    {
        public static IMappingExpression<TLinkedSource, TDestination> MapLinkedSource<TLinkedSource, TDestination>(this IMappingExpression<TLinkedSource, TDestination> expression)
        {
            var mapper = new LinkSourceMapper<TLinkedSource, TDestination>();
            return mapper.MapLinkedSource(expression);
        }

    }
}
