#region copyright
// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#endregion

using AutoMapper;
using System;
using System.Linq.Expressions;

namespace LinkIt.AutoMapperExtensions
{
    public static class MappingExpressionExtensions
    {
        public static IMappingExpression<TLinkedSource, TDestination> MapLinkedSource<TLinkedSource, TDestination>(this IMappingExpression<TLinkedSource, TDestination> expression)
        {
            var mapper = new LinkSourceMapper<TLinkedSource, TDestination>();
            return mapper.MapLinkedSource(expression);
        }

        public static IMappingExpression<TLinkedSource, TDestination> MapLinkedSource<TLinkedSource, TDestination>(
            this IMappingExpression<TLinkedSource, TDestination> expression,
            Expression<Func<TLinkedSource, object>> sourceProperty)
        {
            var mapper = new LinkSourceMapper<TLinkedSource, TDestination>(sourceProperty);
            return mapper.MapLinkedSource(expression);
        }
    }
}
