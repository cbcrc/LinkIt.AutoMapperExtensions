using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;

namespace RC.AutoMapper
{
    public static class MappingExpressionExtensions
    {
        private const string ModelPropertyName = "Model";
        private const string ModelContextualizationPropertyName = "ModelContextualization";

        public static IMappingExpression<TLinkedSource, TDestination> MapLinkedSource<TLinkedSource, TDestination>(this IMappingExpression<TLinkedSource, TDestination> expression)
        {
            EnsureHasModelProperty<TLinkedSource>();
            EnsureHasModelPropertyWhichIsAClass<TLinkedSource>();

            var propertyNameComparer = new PropertyNameComparer();
            var modelPropertiesToMap = GetMappedBySameNameConventionProperties<TLinkedSource, TDestination>();
            var contexualizedProperties = GetModelContextualizationProperties<TLinkedSource>();

            var modelSimplePropertiesToMap = modelPropertiesToMap.Except(contexualizedProperties, propertyNameComparer);
            MapModelProperties(modelSimplePropertiesToMap, expression);

            var modelContextualizationPropertiesToMap = modelPropertiesToMap.Intersect(contexualizedProperties, propertyNameComparer);
            MapModelContextualizedProperties(modelContextualizationPropertiesToMap, expression);

            return expression;
        }

        private static IEnumerable<PropertyInfo> GetMappedBySameNameConventionProperties<TLinkedSource, TDestination>()
        {
            var linkedSourceType = typeof(TLinkedSource);
            var modelType = linkedSourceType.GetProperty(ModelPropertyName).PropertyType;
            var modelProperties = modelType.GetProperties();

            var referenceProperties = linkedSourceType.GetProperties()
                .Where(property => property.Name != ModelPropertyName)
                .ToList();

            var destinationType = typeof(TDestination);
            var destinationProperties = destinationType.GetProperties();

            var propertyNameComparer = new PropertyNameComparer();
            return modelProperties
                .Intersect(destinationProperties, propertyNameComparer)
                .Except(referenceProperties, propertyNameComparer)
                .ToList();
        }

        private static IEnumerable<PropertyInfo> GetModelContextualizationProperties<TLinkedSource>()
        {
            var linkedSourceType = typeof(TLinkedSource);
            var modelContextualization = linkedSourceType.GetProperty(ModelContextualizationPropertyName);
            if (modelContextualization == null)
            {
                return Enumerable.Empty<PropertyInfo>();
            }

            var modelContextualizationType = modelContextualization.PropertyType;
            var modelContextualizationProperties = modelContextualizationType.GetProperties()
                .Where(property => !property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)) // By convention, we don't override the Id using the contextualization
                .ToList();

            return modelContextualizationProperties;
        }

        private static void MapModelProperties<TLinkedSource, TDestination>(
            IEnumerable<PropertyInfo> modelProperties,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            foreach (var modelProperty in modelProperties)
            {
                var method = typeof(MappingExpressionExtensions).GetMethod("MapModelProperty");
                var genericMethod = method.MakeGenericMethod(
                    typeof(TLinkedSource),
                    modelProperty.PropertyType,
                    typeof(TDestination)
                );
                genericMethod.Invoke(null, new object[]
                {
                    modelProperty.Name,
                    expression
                });
            }
        }

        private static void MapModelContextualizedProperties<TLinkedSource, TDestination>(
            IEnumerable<PropertyInfo> modelProperties,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            foreach (var modelProperty in modelProperties)
            {
                var method = typeof(MappingExpressionExtensions).GetMethod("MapModelContextualizedProperty");
                var genericMethod = method.MakeGenericMethod(
                    typeof(TLinkedSource),
                    modelProperty.PropertyType,
                    typeof(TDestination)
                );
                genericMethod.Invoke(null, new object[]
                {
                    modelProperty.Name,
                    expression
                });
            }
        }


        public static void MapModelProperty<TLinkedSource, TSourceProperty, TDestination>(string propertyName,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            var sourcePropertyInDotNotation = string.Format("{0}.{1}", ModelPropertyName, propertyName);
            var memberExpression = CreateMemberExpression<TLinkedSource, TSourceProperty>(sourcePropertyInDotNotation);

            expression.ForMember(propertyName, opt => opt.MapFrom(memberExpression));
        }

        public static void MapModelContextualizedProperty<TLinkedSource, TSourceProperty, TDestination>(string propertyName,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            var contextualizationFunc = CreateContextualizationFunc<TLinkedSource, TSourceProperty>(propertyName);
            expression.ForMember(propertyName, opt => opt.ResolveUsing(contextualizationFunc));
        }


        private static Expression<Func<T, TProperty>> CreateMemberExpression<T, TProperty>(string propertyInDotNotation)
        {
            var root = Expression.Parameter(typeof(T), "root");
            var lambdaBody = GenerateGetProperty(root, propertyInDotNotation);
            return Expression.Lambda<Func<T, TProperty>>(lambdaBody, root);
        }
        
        private static Func<T, object> CreateContextualizationFunc<T, TProperty>(string propertyName)
        {
            var overridingPropertyInDotNotation = string.Format("{0}.{1}", ModelContextualizationPropertyName, propertyName);
            var defaultPropertyInDotNotation = string.Format("{0}.{1}", ModelPropertyName, propertyName);

            var root = Expression.Parameter(typeof(T), "root");

            var contextualizationProperty = GenerateGetProperty(root, ModelContextualizationPropertyName);

            // Create comparison: root.ModelContextualization == null
            var nullExpression = Expression.Constant(null, contextualizationProperty.Type);
            var isContextualizationNull = Expression.Equal(contextualizationProperty, nullExpression);

            // Create call: Contextualize(root.ModelContextualization.Property, root.Model.Property)
            var overridingProperty = GenerateGetProperty(root, overridingPropertyInDotNotation);
            var defaultProperty = GenerateGetProperty(root, defaultPropertyInDotNotation);
            var contextualize = Expression.Call(typeof(MappingExpressionExtensions), "Contextualize", new[] { typeof(TProperty) }, overridingProperty, defaultProperty);

            // Create: root.ModelContextualization == null ? root.Model.Property : Contextualize(root.ModelContextualization.Property, root.Model.Property)
            var defaultOrContextualize = Expression.Condition(isContextualizationNull, defaultProperty, contextualize);

            var x = Expression.Convert(defaultOrContextualize, typeof(object));
            return Expression.Lambda<Func<T, object>>(x, root).Compile();
        }


        private static Expression GenerateGetProperty(ParameterExpression root, string propertyInDotNotation)
        {
            Expression propertyExpression = root;
            foreach (var property in propertyInDotNotation.Split('.'))
            {
                propertyExpression = Expression.PropertyOrField(propertyExpression, property);
            }
            return propertyExpression;
        }

        public static T Contextualize<T>(T overridingValue, T defaultValue)
        {
            return Equals(overridingValue, default(T))
                ? defaultValue
                : overridingValue;
        }


        private static void EnsureHasModelProperty<TLinkedSource>()
        {
            var linkedSourceType = typeof(TLinkedSource);
            var linkedSourceTypeFullName = linkedSourceType.FullName;
            if (linkedSourceType.GetProperty(ModelPropertyName) == null)
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
            var modelType = linkedSourceType.GetProperty(ModelPropertyName).PropertyType;
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
