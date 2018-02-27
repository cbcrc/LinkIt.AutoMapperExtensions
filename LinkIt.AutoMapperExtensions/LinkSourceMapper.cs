#region copyright

// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;

namespace LinkIt.AutoMapperExtensions
{
    public class LinkSourceMapper<TLinkedSource, TDestination>
    {
        private const string ModelPropertyName = "Model";
        private const string ContextualizationPropertyName = "Contextualization";

        private static readonly Type LinkedSourceType = typeof(TLinkedSource);

        private readonly PropertyNameComparer _propertyNameComparer = new PropertyNameComparer();

        private readonly string _sourcePropertyPath = ModelPropertyName;
        private readonly List<PropertyInfo> _sourceProperties;
        private readonly List<PropertyInfo> _destinationProperties;
        private readonly List<PropertyInfo> _referenceProperties;
        private readonly List<PropertyInfo> _contextualizationProperties;

        #region Construction

        public LinkSourceMapper()
        {
            EnsureHasModelProperty();
            EnsureHasModelPropertyWhichIsAClass();

            _sourceProperties = GetModelProperties();
            _destinationProperties = GetDestinationProperties();
            _referenceProperties = GetReferenceProperties();
            _contextualizationProperties = GetContextualizationProperties();
        }

        public LinkSourceMapper(Expression<Func<TLinkedSource, object>> sourceProperty)
        {
            if (!(sourceProperty.Body is MemberExpression me))
            {
                throw new ArgumentException("Expression must be of type System.Linq.Expressions.MemberExpression", "propertyExpression");
            }

            _sourcePropertyPath = GetPropertiesPrefix(me);
            _sourceProperties = me.Type.GetProperties().ToList();
            _destinationProperties = GetDestinationProperties();
            _referenceProperties = GetReferenceProperties();
            _contextualizationProperties = GetContextualizationProperties();
        }

        private static List<PropertyInfo> GetModelProperties()
        {
            var modelType = LinkedSourceType.GetProperty(ModelPropertyName).PropertyType;
            return modelType.GetProperties().ToList();
        }

        private static List<PropertyInfo> GetDestinationProperties()
        {
            return typeof(TDestination).GetProperties().ToList();
        }

        private static List<PropertyInfo> GetReferenceProperties()
        {
            return LinkedSourceType.GetProperties()
                .Where(property => property.Name != ModelPropertyName)
                .Where(property => property.Name != ContextualizationPropertyName)
                .ToList();
        }

        private static List<PropertyInfo> GetContextualizationProperties()
        {
            var modelContextualization = LinkedSourceType.GetProperty(ContextualizationPropertyName);
            if (modelContextualization == null)
            {
                return new List<PropertyInfo>();
            }

            var modelContextualizationType = modelContextualization.PropertyType;
            return modelContextualizationType.GetProperties()
                // By convention, we don't override the Id using the contextualization
                .Where(property => !property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private static void EnsureHasModelProperty()
        {
            if (LinkedSourceType.GetProperty(ModelPropertyName) == null)
                throw new ArgumentException(
                    $"{LinkedSourceType.FullName} must have a property named Model, otherwise it cannot be used as a linked source.",
                    nameof(TLinkedSource)
                );
        }

        private static void EnsureHasModelPropertyWhichIsAClass()
        {
            var linkedSourceTypeFullName = LinkedSourceType.FullName;
            var modelType = LinkedSourceType.GetProperty(ModelPropertyName).PropertyType;
            if (modelType.IsClass == false)
                throw new ArgumentException(
                    $"{linkedSourceTypeFullName} must have a property named Model which is a class, otherwise it cannot be used as a linked source.",
                    nameof(TLinkedSource)
                );
        }

        #endregion


        public IMappingExpression<TLinkedSource, TDestination> MapLinkedSource(IMappingExpression<TLinkedSource, TDestination> expression)
        {
            MapModelProperties(expression);
            MapContextualizedModelProperties(expression);
            MapPropertiesAddedInContextualization(expression);

            return expression;
        }

        private static string GetPropertiesPrefix(MemberExpression me)
        {
            var propertiesPrefix = "";

            while (me != null)
            {
                propertiesPrefix = string.IsNullOrEmpty(propertiesPrefix) ?
                    me.Member.Name :
                    $"{me.Member.Name}.{propertiesPrefix}";
                me = me.Expression as MemberExpression;
            }

            return propertiesPrefix;
        }
        private void MapModelProperties(IMappingExpression<TLinkedSource, TDestination> expression)
        {
            var modelPropertiesToMap = _sourceProperties
                .Intersect(_destinationProperties, _propertyNameComparer)
                .Except(_referenceProperties, _propertyNameComparer)
                .Except(_contextualizationProperties, _propertyNameComparer);

            MapNestedProperties(_sourcePropertyPath, modelPropertiesToMap, expression);
        }

        private void MapContextualizedModelProperties(IMappingExpression<TLinkedSource, TDestination> expression)
        {
            var contextualizedModelPropertiesToMap = _sourceProperties
                .Intersect(_destinationProperties, _propertyNameComparer)
                .Intersect(_contextualizationProperties, _propertyNameComparer)
                .Except(_referenceProperties, _propertyNameComparer);

            MapContextualizedProperties(contextualizedModelPropertiesToMap, expression);
        }

        private void MapPropertiesAddedInContextualization(IMappingExpression<TLinkedSource, TDestination> expression)
        {
            var propertiesAddedInContextualization = _contextualizationProperties
                .Intersect(_destinationProperties, _propertyNameComparer)
                .Except(_referenceProperties, _propertyNameComparer)
                .Except(_sourceProperties, _propertyNameComparer);

            MapNestedProperties(ContextualizationPropertyName, propertiesAddedInContextualization, expression);
        }

        #region MapNestedProperties

        private static void MapNestedProperties(
            string sourcePropertiesPrefix,
            IEnumerable<PropertyInfo> nestedProperties,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            foreach (var property in nestedProperties)
            {
                var sourcePropertyInDotNotation = $"{sourcePropertiesPrefix}.{property.Name}";
                var method = ThisType.GetMethod("MapProperty");
                var genericMethod = method.MakeGenericMethod(property.PropertyType);

                genericMethod.Invoke(null, new object[]
                {
                    sourcePropertyInDotNotation,
                    property.Name,
                    expression
                });
            }
        }

        public static void MapProperty<TSourceProperty>(
            string sourcePropertyInDotNotation,
            string destinationPropertyName,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            var memberExpression = CreateMemberExpression<TSourceProperty>(sourcePropertyInDotNotation);

            expression.ForMember(destinationPropertyName, opt => opt.MapFrom(memberExpression));
        }

        private static Expression<Func<TLinkedSource, TProperty>> CreateMemberExpression<TProperty>(string propertyInDotNotation)
        {
            var root = Expression.Parameter(LinkedSourceType, "root");
            var lambdaBody = GenerateGetProperty(root, propertyInDotNotation);
            return Expression.Lambda<Func<TLinkedSource, TProperty>>(lambdaBody, root);
        }

        #endregion

        #region MapContextualizedProperties

        private static void MapContextualizedProperties(
            IEnumerable<PropertyInfo> contextualizedProperties,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            foreach (var property in contextualizedProperties)
            {
                var overridingPropertyInDotNotation = string.Format("{0}.{1}", ContextualizationPropertyName, property.Name);
                var defaultPropertyInDotNotation = string.Format("{0}.{1}", ModelPropertyName, property.Name);

                var method = ThisType.GetMethod("MapContextualizedProperty");
                var genericMethod = method.MakeGenericMethod(property.PropertyType);
                genericMethod.Invoke(null, new object[]
                {
                    overridingPropertyInDotNotation,
                    defaultPropertyInDotNotation,
                    property.Name,
                    expression
                });
            }
        }

        public static void MapContextualizedProperty<TSourceProperty>(
            string overridingPropertyInDotNotation,
            string defaultPropertyInDotNotation,
            string destinationPropertyName,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            var contextualizationFunc = CreateContextualizationFunc<TSourceProperty>(overridingPropertyInDotNotation, defaultPropertyInDotNotation);
            expression.ForMember(destinationPropertyName, opt => opt.ResolveUsing(contextualizationFunc));
        }

        private static Func<TLinkedSource, TProperty> CreateContextualizationFunc<TProperty>(string overridingPropertyInDotNotation, string defaultPropertyInDotNotation)
        {
            var root = Expression.Parameter(LinkedSourceType, "root");

            var contextualizationProperty = GenerateGetProperty(root, ContextualizationPropertyName);

            // Create comparison: root.Contextualization == null
            var nullExpression = Expression.Constant(null, contextualizationProperty.Type);
            var isContextualizationNull = Expression.Equal(contextualizationProperty, nullExpression);

            // Create call: Contextualize(root.Contextualization.Property, root.Model.Property)
            var overridingProperty = GenerateGetProperty(root, overridingPropertyInDotNotation);
            var defaultProperty = GenerateGetProperty(root, defaultPropertyInDotNotation);

            var contextualizeFuncName = GetContextualizeFuncNameToCall<TProperty>();

            var contextualize = Expression.Call(
                ThisType,
                contextualizeFuncName,
                new[] { typeof(TProperty) },
                overridingProperty,
                defaultProperty
            );

            // Create: root.Contextualization == null ? root.Model.Property : Contextualize(root.Contextualization.Property, root.Model.Property)
            var defaultOrContextualize = Expression.Condition(isContextualizationNull, defaultProperty, contextualize);

            return Expression.Lambda<Func<TLinkedSource, TProperty>>(defaultOrContextualize, root).Compile();
        }

        private static string GetContextualizeFuncNameToCall<TProperty>()
        {
            var tProperty = typeof(TProperty);

            if (tProperty.IsValueType && !IsNullableType(tProperty)) return "ContextualizeValueType";
            return "Contextualize";
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static T Contextualize<T>(T overridingValue, T defaultValue = default)
        {
            return OverrideConvention.IsOverridden(overridingValue)
                ? overridingValue
                : defaultValue;
        }

        public static T ContextualizeValueType<T>(T? overridingValue, T defaultValue) where T : struct
        {
            return OverrideConvention.IsOverridden(overridingValue)
                ? overridingValue.Value
                : defaultValue;
        }

        #endregion

        #region Misc

        private static Expression GenerateGetProperty(ParameterExpression root, string propertyInDotNotation)
        {
            Expression propertyExpression = root;
            foreach (var property in propertyInDotNotation.Split('.')) propertyExpression = Expression.PropertyOrField(propertyExpression, property);
            return propertyExpression;
        }

        private static Type ThisType => typeof(LinkSourceMapper<TLinkedSource, TDestination>);

        #endregion
    }
}