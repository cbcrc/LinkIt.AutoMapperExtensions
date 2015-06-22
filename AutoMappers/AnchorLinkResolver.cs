using System;
using System.Linq.Expressions;
using AutoMapper;
using RC.Neuro.Web.UrlTemplates;

namespace RC.Scoop.Web.Api.AutoMappers
{
    public class AnchorLinkResolver<TSource>: IValueResolver
    {
        private readonly Func<TSource, string> _fragmentExpression;

        public AnchorLinkResolver(Expression<Func<TSource, string>> fragmentExpression)
        {
            _fragmentExpression = fragmentExpression.Compile();
        }

        public ResolutionResult Resolve(ResolutionResult source)
        {
            if (!source.Context.Options.Items.ContainsKey("RootUrlTemplate"))
            {
                return source.New(null);
            }

            var urlTemplate = (UrlTemplate)source.Context.Options.Items["RootUrlTemplate"];
            urlTemplate.SetParameter("fragment", _fragmentExpression.Invoke((TSource)source.Value));

            return source.New(urlTemplate.Resolve());
        }
    }
}