using System;
using System.Linq.Expressions;
using AutoMapper;
using ShowMeAnExampleOfAutomapperFromLinkedSource.UrlTemplates;

namespace ShowMeAnExampleOfAutomapperFromLinkedSource.AutoMappers
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

            var rootUrlTemplate = (UrlTemplate)source.Context.Options.Items["RootUrlTemplate"];
            var urlTemplate = rootUrlTemplate.DeepCopy();
            urlTemplate.SetParameter("fragment", _fragmentExpression.Invoke((TSource)source.Value));

            return source.New(urlTemplate.Resolve());
        }
    }
}