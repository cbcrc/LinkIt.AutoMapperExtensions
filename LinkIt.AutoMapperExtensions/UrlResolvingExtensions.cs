using System;
using System.Collections.Generic;
using AutoMapper;
using LinkIt.AutoMapperExtensions.UrlTemplates;

namespace LinkIt.AutoMapperExtensions
{
    public static class UrlResolvingExtensions
    {
        public static void ResolveUrl<TSource>(this IMemberConfigurationExpression<TSource> member, string webApiRouteTemplate, Func<TSource, object> id)
        {
            member.ResolveUsing(source => new UrlTemplate(webApiRouteTemplate, id(source)).Resolve());
        }

        public static void ResolveUrl<TSource>(this IMemberConfigurationExpression<TSource> member, string webApiRouteTemplate, Func<TSource, IDictionary<string, object>> urlParameters)
        {
            member.ResolveUsing(source => new UrlTemplate(webApiRouteTemplate, urlParameters(source)).Resolve());
        }
    }
}