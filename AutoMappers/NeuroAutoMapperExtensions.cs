using System;
using System.Collections.Generic;
using AutoMapper;
using ShowMeAnExampleOfAutomapperFromLinkedSource.UrlTemplates;

namespace ShowMeAnExampleOfAutomapperFromLinkedSource.AutoMappers
{
    public static class NeuroAutoMapperExtensions
    {
        public static void ResolveUrl<TSource>(this IMemberConfigurationExpression<TSource> member, string neuroWebApiRouteTemplate, Func<TSource, int?> idFunc)
        {
            member.ResolveUsing(source => new UrlTemplate(neuroWebApiRouteTemplate, idFunc(source)).Resolve());
        }

        public static void ResolveUrl<TSource>(this IMemberConfigurationExpression<TSource> member, string neuroWebApiRouteTemplate, Func<TSource, IDictionary<string, object>> urlParametersFunc)
        {
            member.ResolveUsing(source => new UrlTemplate(neuroWebApiRouteTemplate, urlParametersFunc(source)).Resolve());
        }
    }
}