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
        private List<PropertyInfo> _contextualizationProperties;
        private List<PropertyInfo> _destinationProperties;

        private List<PropertyInfo> _modelProperties;
        private readonly PropertyNameComparer _propertyNameComparer;
        private List<PropertyInfo> _referenceProperties;

        public IMappingExpression<TLinkedSource, TDestination> MapLinkedSource(IMappingExpression<TLinkedSource, TDestination> expression)
        {
            MapModelProperties(expression);
            MapContextualizedModelProperties(expression);
            MapPropertiesAddedInContextualization(expression);

            return expression;
        }

        public IMappingExpression<TLinkedSource, TDestination> MapNestedProperties(
            IMappingExpression<TLinkedSource, TDestination> expression,
            Expression<Func<TLinkedSource, object>> propertyExpression)
        {
            var me = propertyExpression.Body as MemberExpression;
            if (me == null)
            {
                throw new ArgumentException("Expression must be of type System.Linq.Expressions.MemberExpression", "propertyExpression");
            }

            var propertiesPrefix = GetPropertiesPrefix(me);
            var nestedProperties = me.Type.GetProperties().ToList();

            var modelPropertiesToMap = _destinationProperties
                .Intersect(nestedProperties, _propertyNameComparer)
                .Except(_referenceProperties, _propertyNameComparer)
                .Except(_contextualizationProperties, _propertyNameComparer);

            MapNestedProperties(propertiesPrefix, modelPropertiesToMap, expression);

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
            var modelPropertiesToMap = _modelProperties
                .Intersect(_destinationProperties, _propertyNameComparer)
                .Except(_referenceProperties, _propertyNameComparer)
                .Except(_contextualizationProperties, _propertyNameComparer);

            MapNestedProperties(ModelPropertyName, modelPropertiesToMap, expression);
        }

        private void MapContextualizedModelProperties(IMappingExpression<TLinkedSource, TDestination> expression)
        {
            var contextualizedModelPropertiesToMap = _modelProperties
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
                .Except(_modelProperties, _propertyNameComparer);

            MapNestedProperties(ContextualizationPropertyName, propertiesAddedInContextualization, expression);
        }

        #region Construction

        public LinkSourceMapper()
        {
            EnsureHasModelProperty();
            EnsureHasModelPropertyWhichIsAClass();

            _propertyNameComparer = new PropertyNameComparer();
            InitModelProperties();
            InitDestinationProperties();
            InitReferenceProperties();
            InitContextualizationProperties();
        }

        private void InitModelProperties()
        {
            var linkedSourceType = typeof(TLinkedSource);
            var modelType = linkedSourceType.GetProperty(ModelPropertyName).PropertyType;
            _modelProperties = modelType.GetProperties().ToList();
        }

        private void InitDestinationProperties()
        {
            var destinationType = typeof(TDestination);
            _destinationProperties = destinationType.GetProperties().ToList();
        }

        private void InitReferenceProperties()
        {
            var linkedSourceType = typeof(TLinkedSource);

            _referenceProperties = linkedSourceType.GetProperties()
                .Where(property => property.Name != ModelPropertyName)
                .Where(property => property.Name != ContextualizationPropertyName)
                .ToList();
        }

        private void InitContextualizationProperties()
        {
            var linkedSourceType = typeof(TLinkedSource);
            var modelContextualization = linkedSourceType.GetProperty(ContextualizationPropertyName);
            if (modelContextualization == null)
            {
                _contextualizationProperties = new List<PropertyInfo>();
            }
            else
            {
                var modelContextualizationType = modelContextualization.PropertyType;
                _contextualizationProperties = modelContextualizationType.GetProperties()
                    // By convention, we don't override the Id using the contextualization
                    .Where(property => !property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        private static void EnsureHasModelProperty()
        {
            var linkedSourceType = typeof(TLinkedSource);
            var linkedSourceTypeFullName = linkedSourceType.FullName;
            if (linkedSourceType.GetProperty(ModelPropertyName) == null)
                throw new ArgumentException(
                    string.Format(
                        "{0} must have a property named Model, otherwise it cannot be used as a linked source.",
                        linkedSourceTypeFullName
                    ),
                    "TLinkedSource"
                );
        }

        private static void EnsureHasModelPropertyWhichIsAClass()
        {
            var linkedSourceType = typeof(TLinkedSource);
            var linkedSourceTypeFullName = linkedSourceType.FullName;
            var modelType = linkedSourceType.GetProperty(ModelPropertyName).PropertyType;
            if (modelType.IsClass == false)
                throw new ArgumentException(
                    string.Format(
                        "{0} must have a property named Model which is a class, otherwise it cannot be used as a linked source.",
                        linkedSourceTypeFullName
                    ),
                    "TLinkedSource"
                );
        }

        #endregion

        #region MapNestedProperties

        private static void MapNestedProperties(
            string sourcePropertiesPrefix,
            IEnumerable<PropertyInfo> nestedProperties,
            IMappingExpression<TLinkedSource, TDestination> expression)
        {
            foreach (var property in nestedProperties)
            {
                var sourcePropertyInDotNotation = string.Format("{0}.{1}", sourcePropertiesPrefix, property.Name);
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
            var root = Expression.Parameter(typeof(TLinkedSource), "root");
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
            var root = Expression.Parameter(typeof(TLinkedSource), "root");

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