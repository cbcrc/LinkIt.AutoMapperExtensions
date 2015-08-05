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
        private const string ModelPropertyName = "Model";
        private const string ContextualizationPropertyName = "Contextualization";

        public static IMappingExpression<TLinkedSource, TDestination> MapLinkedSource<TLinkedSource, TDestination>(this IMappingExpression<TLinkedSource, TDestination> expression)
        {
            EnsureHasModelProperty<TLinkedSource>();
            EnsureHasModelPropertyWhichIsAClass<TLinkedSource>();

            var propertyNameComparer = new PropertyNameComparer();

            var modelProperties = GetModelProperties<TLinkedSource>();
            var destinationProperties = GetDestinationProperties<TDestination>();
            var referenceProperties = GetReferenceProperties<TLinkedSource>();
            var contextualizationProperties = GetContextualizationProperties<TLinkedSource>();

            var modelPropertiesToMap = modelProperties
                .Intersect(destinationProperties, propertyNameComparer)
                .Except(referenceProperties, propertyNameComparer)
                .Except(contextualizationProperties, propertyNameComparer);
            MapModelProperties(modelPropertiesToMap, expression);

            var contextualizedModelPropertiesToMap = modelProperties
                .Intersect(destinationProperties, propertyNameComparer)
                .Intersect(contextualizationProperties, propertyNameComparer)
                .Except(referenceProperties, propertyNameComparer);
            MapContextualizedModelProperties(contextualizedModelPropertiesToMap, expression);

            var contextualizationPropertiesToMap = contextualizationProperties
                .Intersect(destinationProperties, propertyNameComparer)
                .Except(referenceProperties, propertyNameComparer)
                .Except(modelProperties, propertyNameComparer);
            MapContextualizationProperties(contextualizationPropertiesToMap, expression);

            var contextualizedReferencePropertiesToMap = contextualizationProperties
                .Intersect(destinationProperties, propertyNameComparer)
                .Intersect(referenceProperties, propertyNameComparer);
            MapContextualizedReferenceProperties(contextualizedReferencePropertiesToMap, expression);

            return expression;
        }

        private static IEnumerable<PropertyInfo> GetModelProperties<TLinkedSource>()
        {
            var linkedSourceType = typeof(TLinkedSource);
            var modelType = linkedSourceType.GetProperty(ModelPropertyName).PropertyType;
            return modelType.GetProperties();
        }

        private static IEnumerable<PropertyInfo> GetDestinationProperties<TDestination>()
        {
            var destinationType = typeof(TDestination);
            return destinationType.GetProperties();
        }

        private static IEnumerable<PropertyInfo> GetReferenceProperties<TLinkedSource>()
        {
            var linkedSourceType = typeof(TLinkedSource);

            return linkedSourceType.GetProperties()
                .Where(property => property.Name != ModelPropertyName)
                .Where(property => property.Name != ContextualizationPropertyName)
                .ToList();
        }

        private static IEnumerable<PropertyInfo> GetContextualizationProperties<TLinkedSource>()
        {
            var linkedSourceType = typeof(TLinkedSource);
            var modelContextualization = linkedSourceType.GetProperty(ContextualizationPropertyName);
            if (modelContextualization == null)
            {
                return Enumerable.Empty<PropertyInfo>();
            }

            var modelContextualizationType = modelContextualization.PropertyType;
            return modelContextualizationType.GetProperties()
                .Where(property => !property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)) // By convention, we don't override the Id using the contextualization
                .ToList();
        }

        private static void MapModelProperties<TLinkedSource, TDestination>(
            IEnumerable<PropertyInfo> modelProperties,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            MapNestedProperties(ModelPropertyName, modelProperties, expression);
        }

        private static void MapContextualizationProperties<TLinkedSource, TDestination>(
            IEnumerable<PropertyInfo> contextualizationPropertiesToMap, 
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            MapNestedProperties(ContextualizationPropertyName, contextualizationPropertiesToMap, expression);
        }

        private static void MapNestedProperties<TLinkedSource, TDestination>(
            string sourcePropertiesPrefix,
            IEnumerable<PropertyInfo> nestedProperties,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            foreach (var property in nestedProperties)
            {
                var sourcePropertyInDotNotation = string.Format("{0}.{1}", sourcePropertiesPrefix, property.Name);
                var method = typeof(MappingExpressionExtensions).GetMethod("MapProperty");
                var genericMethod = method.MakeGenericMethod(
                    typeof(TLinkedSource),
                    property.PropertyType,
                    typeof(TDestination)
                );
                genericMethod.Invoke(null, new object[]
                {
                    sourcePropertyInDotNotation,
                    property.Name,
                    expression
                });
            }
        }

        private static void MapContextualizedModelProperties<TLinkedSource, TDestination>(
            IEnumerable<PropertyInfo> properties,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            MapContextualizedProperties(ModelPropertyName, properties, expression);
        }

        private static void MapContextualizedReferenceProperties<TLinkedSource, TDestination>(
            IEnumerable<PropertyInfo> properties,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            MapContextualizedProperties(null, properties, expression);
        }

        private static void MapContextualizedProperties<TLinkedSource, TDestination>(
            string defaultPropertiesPrefix,
            IEnumerable<PropertyInfo> contextualizedProperties,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            foreach (var property in contextualizedProperties)
            {
                var overridingPropertyInDotNotation = string.Format("{0}.{1}", ContextualizationPropertyName, property.Name);
                var defaultPropertyInDotNotation = defaultPropertiesPrefix == null ? property.Name : string.Format("{0}.{1}", defaultPropertiesPrefix, property.Name);

                var method = typeof(MappingExpressionExtensions).GetMethod("MapContextualizedProperty");
                var genericMethod = method.MakeGenericMethod(
                    typeof(TLinkedSource),
                    property.PropertyType,
                    typeof(TDestination)
                );
                genericMethod.Invoke(null, new object[]
                {
                    overridingPropertyInDotNotation,
                    defaultPropertyInDotNotation,
                    property.Name,
                    expression
                });
            }
        }
  

        public static void MapProperty<TLinkedSource, TSourceProperty, TDestination>(string sourcePropertyInDotNotation, string destinationPropertyName,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            var memberExpression = CreateMemberExpression<TLinkedSource, TSourceProperty>(sourcePropertyInDotNotation);

            expression.ForMember(destinationPropertyName, opt => opt.MapFrom(memberExpression));
        }

        public static void MapContextualizedProperty<TLinkedSource, TSourceProperty, TDestination>(
            string overridingPropertyInDotNotation, 
            string defaultPropertyInDotNotation,
            string destinationPropertyName,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            var contextualizationFunc = CreateContextualizationFunc<TLinkedSource, TSourceProperty>(overridingPropertyInDotNotation, defaultPropertyInDotNotation);
            expression.ForMember(destinationPropertyName, opt => opt.ResolveUsing(contextualizationFunc));
        }


        private static Expression<Func<T, TProperty>> CreateMemberExpression<T, TProperty>(string propertyInDotNotation)
        {
            var root = Expression.Parameter(typeof(T), "root");
            var lambdaBody = GenerateGetProperty(root, propertyInDotNotation);
            return Expression.Lambda<Func<T, TProperty>>(lambdaBody, root);
        }
        
        private static Func<T, object> CreateContextualizationFunc<T, TProperty>(string overridingPropertyInDotNotation, string defaultPropertyInDotNotation)
        {
            var root = Expression.Parameter(typeof(T), "root");

            var contextualizationProperty = GenerateGetProperty(root, ContextualizationPropertyName);

            // Create comparison: root.Contextualization == null
            var nullExpression = Expression.Constant(null, contextualizationProperty.Type);
            var isContextualizationNull = Expression.Equal(contextualizationProperty, nullExpression);

            // Create call: Contextualize(root.Contextualization.Property, root.Model.Property)
            var overridingProperty = GenerateGetProperty(root, overridingPropertyInDotNotation);
            var defaultProperty = GenerateGetProperty(root, defaultPropertyInDotNotation);
            var contextualize = Expression.Call(typeof(MappingExpressionExtensions), "Contextualize", new[] { typeof(TProperty) }, overridingProperty, defaultProperty);

            // Create: root.Contextualization == null ? root.Model.Property : Contextualize(root.Contextualization.Property, root.Model.Property)
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
