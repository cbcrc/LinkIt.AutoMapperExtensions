using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;

namespace RC.AutoMapper
{
    public static class MappingExpressionExtensions
    {
        public static IMappingExpression<TLinkedSource, TDestination> MapLinkedSource<TLinkedSource, TDestination>(this IMappingExpression<TLinkedSource, TDestination> expression)
        {
            EnsureHasModelProperty<TLinkedSource>();
            EnsureHasModelPropertyWhichIsAClass<TLinkedSource>();

            var modelPropertiesToMap = GetMappedBySameNameConventionProperties<TLinkedSource, TDestination>();
            MapModelProperties(modelPropertiesToMap, expression);

            return expression;
        }

        private static List<PropertyInfo> GetMappedBySameNameConventionProperties<TLinkedSource, TDestination>()
        {

            var linkedSourceType = typeof(TLinkedSource);
            var modelType = linkedSourceType.GetProperty("Model").PropertyType;
            var modelProperties = modelType.GetProperties();

            var referenceProperties = linkedSourceType.GetProperties()
                .Where(property => property.Name != "Model")
                .ToList();

            var destinationType = typeof(TDestination);
            var destinationProperties = destinationType.GetProperties();

            var propertyNameComparer = new PropertyNameComparer();
            return modelProperties
                .Intersect(destinationProperties, propertyNameComparer)
                .Except(referenceProperties, propertyNameComparer)
                .ToList();
        }

        private static void MapModelProperties<TLinkedSource, TDestination>(
            List<PropertyInfo> modelProperties,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            foreach (var modelProperty in modelProperties)
            {
                var method = typeof(MappingExpressionExtensions).GetMethod("MapProperty");
                var genericMethod = method.MakeGenericMethod(
                    typeof(TLinkedSource),
                    modelProperty.PropertyType,
                    typeof(TDestination)
                );
                genericMethod.Invoke(null, new object[] { "Model." + modelProperty.Name, modelProperty.Name, expression });
            }
        }


        public static void MapProperty<TLinkedSource, TSourceProperty, TDestination>(
            string sourcePropertyInDotNotation,
            string destinationPropertyName,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            var memberExpression = CreateMemberExpression<TLinkedSource, TSourceProperty>(sourcePropertyInDotNotation);
            expression.ForMember(destinationPropertyName, opt => opt.MapFrom(memberExpression));
        }

        static Expression<Func<T, TProperty>> CreateMemberExpression<T, TProperty>(string propertyInDotNotation)
        {
            var root = Expression.Parameter(typeof(T), "root");
            Expression lambdaBody = root;
            foreach (var property in propertyInDotNotation.Split('.'))
            {
                lambdaBody = Expression.PropertyOrField(lambdaBody, property);
            }

            return Expression.Lambda<Func<T, TProperty>>(lambdaBody, root);
        }


        private static void EnsureHasModelProperty<TLinkedSource>()
        {
            var linkedSourceType = typeof(TLinkedSource);
            var linkedSourceTypeFullName = linkedSourceType.FullName;
            if (linkedSourceType.GetProperty("Model") == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "{0} must have a property named Model, otherwise it cannot be used as a linked source.",
                        linkedSourceTypeFullName
                    ),
                    "TLinkedSource"
                );
            }
        }

        private static void EnsureHasModelPropertyWhichIsAClass<TLinkedSource>()
        {
            var linkedSourceType = typeof(TLinkedSource);
            var linkedSourceTypeFullName = linkedSourceType.FullName;
            var modelType = linkedSourceType.GetProperty("Model").PropertyType;
            if (modelType.IsClass == false)
            {
                throw new ArgumentException(
                    string.Format(
                        "{0} must have a property named Model which is a class, otherwise it cannot be used as a linked source.",
                        linkedSourceTypeFullName
                    ),
                    "TLinkedSource"
                    );
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
}
