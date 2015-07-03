using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;

namespace ShowMeAnExampleOfAutomapperFromLinkedSource.AutoMappers
{
    public static class MappingExpressionExtensions2
    {
        public static IMappingExpression<TLinkedSource, TDestination> MapModel2<TLinkedSource, TDestination>(this IMappingExpression<TLinkedSource, TDestination> expression)
        {
            EnsureHasModelProperty<TLinkedSource>();
            EnsureHasModelPropertyWhichIsAClass<TLinkedSource>();

            var modelPropertiesToMap = GetMappedBySameNameConventionProperties<TLinkedSource, TDestination>();
            MapModelProperties(modelPropertiesToMap, expression);

            var modelContextualizationPropertiesToMap = GetMappedBySameNameConventionContextualizedProperties<TLinkedSource, TDestination>();
            MapModelContextualizedProperties(modelContextualizationPropertiesToMap, expression);

            return expression;
        }

        private static List<PropertyInfo> GetMappedBySameNameConventionProperties<TLinkedSource, TDestination>()
        {

            var linkedSourceType = typeof(TLinkedSource);
            var modelType = linkedSourceType.GetProperty("Model").PropertyType;
            var modelProperties = modelType.GetProperties();

            var modelContextualizationType = linkedSourceType.GetProperty("ModelContextualization").PropertyType;
            var modelContextualizationProperties = modelContextualizationType.GetProperties();

            var referenceProperties = linkedSourceType.GetProperties()
                .Where(property => property.Name != "Model")
                .ToList();

            var destinationType = typeof(TDestination);
            var destinationProperties = destinationType.GetProperties();

            var propertyNameComparer = new PropertyNameComparer();
            return modelProperties
                .Intersect(destinationProperties, propertyNameComparer)
                .Except(referenceProperties, propertyNameComparer)
                .Except(modelContextualizationProperties, propertyNameComparer)
                .ToList();
        }

        private static List<PropertyInfo> GetMappedBySameNameConventionContextualizedProperties<TLinkedSource, TDestination>() {

            var linkedSourceType = typeof(TLinkedSource);
            var modelType = linkedSourceType.GetProperty("Model").PropertyType;
            var modelProperties = modelType.GetProperties();

            var modelContextualizationType = linkedSourceType.GetProperty("ModelContextualization").PropertyType;
            var modelContextualizationProperties = modelContextualizationType.GetProperties();

            var destinationType = typeof(TDestination);
            var destinationProperties = destinationType.GetProperties();

            var propertyNameComparer = new PropertyNameComparer();
            return modelProperties
                .Intersect(destinationProperties, propertyNameComparer)
                .Intersect(modelContextualizationProperties, propertyNameComparer)
                .ToList();
        }

        private static void MapModelProperties<TLinkedSource, TDestination>(
            List<PropertyInfo> modelProperties,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            foreach (var modelProperty in modelProperties)
            {
                var method = typeof(MappingExpressionExtensions2).GetMethod("MapProperty");
                var genericMethod = method.MakeGenericMethod(
                    typeof(TLinkedSource),
                    modelProperty.PropertyType,
                    typeof(TDestination)
                );
                genericMethod.Invoke(null, new object[] { "Model." + modelProperty.Name, modelProperty.Name, expression });
            }
        }

        private static void MapModelContextualizedProperties<TLinkedSource, TDestination>(
            List<PropertyInfo> modelProperties,
            IMappingExpression<TLinkedSource, TDestination> expression) {
            foreach (var modelProperty in modelProperties) {
                var method = typeof(MappingExpressionExtensions2).GetMethod("MapContextualizedProperty");
                var genericMethod = method.MakeGenericMethod(
                    typeof(TLinkedSource),
                    modelProperty.PropertyType,
                    typeof(TDestination)
                );
                genericMethod.Invoke(null, new object[]
                {
                    "ModelContextualization." + modelProperty.Name,  
                    "Model." + modelProperty.Name, 
                    modelProperty.Name, 
                    expression
                });
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

        public static void MapContextualizedProperty<TLinkedSource, TSourceProperty, TDestination>(
            string overridingPropertyInDotNotation, 
            string defaultPropertyInDotNotation,
            string destinationPropertyName,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            var contextualizationFunc = CreateContextualizationFunc<TLinkedSource, TSourceProperty>(
                overridingPropertyInDotNotation, 
                defaultPropertyInDotNotation
            );
            expression.ForMember(destinationPropertyName, opt => opt.ResolveUsing(contextualizationFunc));
        }


        private static Expression<Func<T, TProperty>> CreateMemberExpression<T, TProperty>(string propertyInDotNotation)
        {
            var root = Expression.Parameter(typeof(T), "root");
            Expression lambdaBody = GenerateGetProperty<T>(root,propertyInDotNotation);
            return Expression.Lambda<Func<T, TProperty>>(lambdaBody, root);
        }

        static Func<T, object> CreateContextualizationFunc<T, TProperty>(string overridingPropertyInDotNotation, string defaultPropertyInDotNotation) {
            var root = Expression.Parameter(typeof(T), "root");
            Expression overridingProperty = GenerateGetProperty<T>(root, overridingPropertyInDotNotation);
            Expression defaultProperty = GenerateGetProperty<T>(root, defaultPropertyInDotNotation);

            var contextualizeFuncInvocation = Expression.Call(typeof(MappingExpressionExtensions2), "ContextualizeFunc", new[] { typeof(TProperty) }, overridingProperty, defaultProperty);
            var x = Expression.Convert(contextualizeFuncInvocation, typeof (object));
            return Expression.Lambda<Func<T, object>>(x, root).Compile();
        }


        private static Expression GenerateGetProperty<T>(ParameterExpression root, string propertyInDotNotation) {
            Expression propertyExpression = root;
            foreach (var property in propertyInDotNotation.Split('.')) {
                propertyExpression = Expression.PropertyOrField(propertyExpression, property);
            }
            return propertyExpression;
        }

        public static T ContextualizeFunc<T>(T overridingValue, T defaultValue)  {
            return Equals(overridingValue,default(T))
                ? defaultValue
                : overridingValue;
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
