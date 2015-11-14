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
