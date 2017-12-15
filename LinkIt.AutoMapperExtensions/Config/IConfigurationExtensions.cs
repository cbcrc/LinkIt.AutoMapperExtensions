#region copyright

// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Linq;
using System.Reflection;
using AutoMapper;

namespace LinkIt.AutoMapperExtensions.Config
{
    public static class ConfigurationExtensions
    {
        public static void ApplyTransformConfigs(this IMapperConfigurationExpression config, params Type[] typesToScanAssemblies)
        {
            if (typesToScanAssemblies == null) throw new ArgumentNullException(nameof(typesToScanAssemblies));

            ApplyTransformConfigs(config, typesToScanAssemblies.Select(t => t.Assembly).ToArray());
        }

        public static void ApplyTransformConfigs(this IMapperConfigurationExpression config, params Assembly[] assemblies)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));

            var transformConfigTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.GetInterface("LinkIt.AutoMapperExtensions.Config.ITransformConfig") != null)
                .ToList();

            var transformConfigs = transformConfigTypes
                .OrderBy(GetOrderByPriorityKey)
                .Select(Activator.CreateInstance)
                .Cast<ITransformConfig>()
                .ToList();

            foreach (var transformConfig in transformConfigs) transformConfig.ConfigureTransformation(config);
        }

        private static string GetOrderByPriorityKey(Type transformConfigType)
        {
            return string.Format(
                "{0}-{1}",
                GetTransformConfigPriority(transformConfigType),
                transformConfigType.FullName
            );
        }

        private static int GetTransformConfigPriority(Type transformConfigType)
        {
            //IAbstractTransformConfig must be configure before ITransformConfig
            //because otherwise the mapping defined at the abstract level are ignored
            //at the implementation level.
            //
            //See WARNING: Inherited mappings are error-prone before using IAbstractTransformConfig

            return transformConfigType.GetInterface("LinkIt.AutoMapperExtensions.Config.IAbstractTransformConfig") != null
                ? 1
                : 2;
        }
    }
}