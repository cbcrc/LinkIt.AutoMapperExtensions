using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
            var modelProperties = modelType.GetProperties();

            var linkedSourceType = typeof(TLinkedSource);
            var referenceProperties = linkedSourceType.GetProperties()
                .Where(property => property.Name!="Model")
                .ToList();

            var destinationType = typeof(TDestination);
            var destinationProperties = destinationType.GetProperties();

            var propertyNameComparer = new PropertyNameComparer();
            var mappedBySameNameConventionProperties = modelProperties
                .Intersect(destinationProperties, propertyNameComparer)
                .Except(referenceProperties, propertyNameComparer)
                .ToList();

            foreach (var property in mappedBySameNameConventionProperties) {
                var method = typeof(MappingExpressionExtensions).GetMethod("BindMember");
                var genericMethod = method.MakeGenericMethod(linkedSourceType, destinationType, property.GetType());
                genericMethod.Invoke(null, new object[] { "Model." + property.Name, expression });
            }

            return expression;
        }

        public static void BindMember<TLinkedSource, TDestination, TReference>(string propertyName, IMappingExpression<TLinkedSource, TDestination> expression) {
            var lambda = CreateGenericExpression<TLinkedSource, TReference>(propertyName);
            expression.ForMember(propertyName, opt => opt.MapFrom(lambda));
        }

        static Expression<Func<T, TReference>> CreateGenericExpression<T, TReference>(string propertyName) {
            var param = Expression.Parameter(typeof(T), "x");
            Expression body = param;
            foreach (var member in propertyName.Split('.')) {
                body = Expression.PropertyOrField(body, member);
            }

//            body = Expression.Convert(body, typeof (TReference));

            return Expression.Lambda<Func<T, TReference>>(body, param);
        }
    }

    public class PropertyNameComparer : IEqualityComparer<PropertyInfo>
    {
        public bool Equals(PropertyInfo x, PropertyInfo y)
        {
            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(PropertyInfo obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
