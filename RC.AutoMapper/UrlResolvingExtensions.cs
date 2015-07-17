using System;
using System.Collections.Generic;
using AutoMapper;
using RC.AutoMapper.UrlTemplates;

namespace RC.AutoMapper
{
    public static class UrlResolvingExtensions
    {
        public static void ResolveUrl<TSource>(this IMemberConfigurationExpression<TSource> member, string webApiRouteTemplate, Func<TSource, int?> id)
        {
            member.ResolveUsing(source => new UrlTemplate(webApiRouteTemplate, id(source)).Resolve());
        }

        public static void ResolveUrl<TSource>(this IMemberConfigurationExpression<TSource> member, string webApiRouteTemplate, Func<TSource, IDictionary<string, object>> urlParameters)
        {
            member.ResolveUsing(source => new UrlTemplate(webApiRouteTemplate, urlParameters(source)).Resolve());
        }
    }
}