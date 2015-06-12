using System;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;

namespace ShowMeAnExampleOfAutomapperFromLinkedSource {
    public static class MappingExpressionExtensions {
        //Useless since it defeat the purpose of AssertConfigurationIsValid
        //However, it is a good inspiration for dynamic mapping expression based on Model
        public static IMappingExpression<TSource, TDestination> IgnoreAllNonExisting<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression) {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);
            var existingMaps = Mapper.GetAllTypeMaps().First(x => x.SourceType.Equals(sourceType)
                && x.DestinationType.Equals(destinationType));
            foreach (var property in existingMaps.GetUnmappedPropertyNames()) {
                expression.ForMember(property, opt => opt.Ignore());
            }
            return expression;
        }

        public static IMappingExpression<TLinkedSource, TDestination> MapModel<TLinkedSource, TModel, TDestination>(this IMappingExpression<TLinkedSource, TDestination> expression) where TLinkedSource: ILinkedSource<TModel>
        {
            var modelType = typeof(TModel);
            var modelPropertyNames = modelType.GetProperties()
                .Select(property => property.Name)
                .ToList();

            var linkedSourceType = typeof(TLinkedSource);
            var referencePropertyNames = linkedSourceType.GetProperties()
                .Select(property => property.Name)
                .Where(propertyName => propertyName!="Model")
                .ToList();

            var destinationType = typeof(TDestination);
            var destinationPropertyNames = destinationType.GetProperties()
                .Select(property => property.Name)
                .ToList();

            var mappedBySameNameConventionPropertyNames = modelPropertyNames
                .Intersect(destinationPropertyNames)
                .Except(referencePropertyNames)
                .ToList();

            foreach (var propertyName in mappedBySameNameConventionPropertyNames)
            {
                //var arg = Expression.Constant(null, typeof(TLinkedSource));
                //var body = Expression.Convert(Expression.PropertyOrField(arg, propertyName),
                //    typeof(object));
                //var lambda = Expression.Lambda<Func<TLinkedSource, object>>(body);

                var lambda = CreateExpression<TLinkedSource>("Model."+propertyName);

                expression.ForMember(propertyName, opt => opt.MapFrom(lambda));
            }

            return expression;
        }

        static Expression<Func<T, object>> CreateExpression<T>(string propertyName) {
            var param = Expression.Parameter(typeof(T), "x");
            Expression body = param;
            foreach (var member in propertyName.Split('.')) {
                body = Expression.PropertyOrField(body, member);
            }
            return Expression.Lambda<Func<T, object>>(body, param);
        }


        static LambdaExpression CreateExpressionAsIs(Type type, string propertyName) {
            var param = Expression.Parameter(type, "x");
            Expression body = param;
            foreach (var member in propertyName.Split('.')) {
                body = Expression.PropertyOrField(body, member);
            }
            return Expression.Lambda(body, param);
        }

    }
}
