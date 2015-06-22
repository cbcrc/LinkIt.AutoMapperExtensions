using System;
using AutoMapper;
using RC.Neuro.Web.UrlTemplates;

namespace RC.Scoop.Web.Api.AutoMappers
{
    public static class NeuroAutoMapperExtensions
    {
        public static void MapFromUrl<TSource>(this IMemberConfigurationExpression<TSource> member, string neuroWebApiRouteTemplate, Func<TSource, int?> idFunc)
        {
            member.MapFrom(source => new UrlTemplate(neuroWebApiRouteTemplate, idFunc(source)).Resolve());
        }

    }
}