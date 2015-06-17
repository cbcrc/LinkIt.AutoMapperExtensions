using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using AutoMapper;

namespace ShowMeAnExampleOfAutomapperFromLinkedSource {
    public static class MappingExpressionExtensions {
        public static IMappingExpression<TLinkedSource, TDestination> MapModel<TLinkedSource, TDestination>(this IMappingExpression<TLinkedSource, TDestination> expression)
        {
            EnsureLinkedSourceHasModelProperty<TLinkedSource>();
            EnsureLinkedSourceIsNonEnumerableClass<TLinkedSource>();


            var mappedBySameNameConventionProperties = GetMappedBySameNameConventionProperties<TLinkedSource, TDestination>();

            foreach (var property in mappedBySameNameConventionProperties) {
                var method = typeof(MappingExpressionExtensions).GetMethod("BindMember");
                var genericMethod = method.MakeGenericMethod(typeof(TLinkedSource), typeof(TDestination), property.PropertyType);
                genericMethod.Invoke(null, new object[] { "Model." + property.Name, property.Name, expression });
            }

            return expression;
        }

        private static List<PropertyInfo> GetMappedBySameNameConventionProperties<TLinkedSource, TDestination>() {
            
            var linkedSourceType = typeof(TLinkedSource);
            var modelType = linkedSourceType.GetProperty("Model").PropertyType;
            var modelProperties = modelType.GetProperties();

            var referenceProperties = linkedSourceType.GetProperties()
                .Where(property => property.Name != "Model")
                .ToList();

            var destinationType = typeof (TDestination);
            var destinationProperties = destinationType.GetProperties();

            var propertyNameComparer = new PropertyNameComparer();
            return modelProperties
                .Intersect(destinationProperties, propertyNameComparer)
                .Except(referenceProperties, propertyNameComparer)
                .ToList();
        }

        public static void BindMember<TLinkedSource, TDestination, TReference>(string sourcePropertyName, string destinationPropertyName, IMappingExpression<TLinkedSource, TDestination> expression) {
            var lambda = CreateGenericExpression<TLinkedSource, TReference>(sourcePropertyName);
            expression.ForMember(destinationPropertyName, opt => opt.MapFrom(lambda));
        }

        static Expression<Func<T, TReference>> CreateGenericExpression<T, TReference>(string propertyName) {
            var param = Expression.Parameter(typeof(T), "x");
            Expression body = param;
            foreach (var member in propertyName.Split('.')) {
                body = Expression.PropertyOrField(body, member);
            }

            return Expression.Lambda<Func<T, TReference>>(body, param);
        }

        private static void EnsureLinkedSourceHasModelProperty<TLinkedSource>() {
            var linkedSourceType = typeof(TLinkedSource);
            if (linkedSourceType.GetProperty("Model") == null) {
                throw new ArgumentException(
                    "TLinkedSource must have a property named Model, otherwise it cannot be used as a linked source.",
                    "TLinkedSource"
                );
            }
        }

        private static void EnsureLinkedSourceIsNonEnumerableClass<TLinkedSource>() {
            var linkedSourceType = typeof(TLinkedSource);
            var modelType = linkedSourceType.GetProperty("Model").PropertyType;
            if (modelType.IsClass == false && typeof(IEnumerable).IsAssignableFrom(modelType) == false) {
                throw new ArgumentException(
                    "TLinkedSource must be a class, otherwise it cannot be used as a linked source.",
                    "TLinkedSource"
                    );
            }
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
