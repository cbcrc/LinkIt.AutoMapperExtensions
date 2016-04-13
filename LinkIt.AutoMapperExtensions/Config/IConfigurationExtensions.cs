using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;

namespace LinkIt.AutoMapperExtensions.Config {
    public static class IConfigurationExtensions {

        public static void ApplyTransformConfigs(this IConfiguration config, IEnumerable<Assembly> assemblies) {
            if (config == null) { throw new ArgumentNullException("config"); }
            if (assemblies == null) { throw new ArgumentNullException("assemblies"); }

            var transformConfigTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.GetInterface("LinkIt.AutoMapperExtensions.Config.ITransformConfig") != null)
                .ToList();

            var transformConfigs = transformConfigTypes
                .OrderBy(GetOrderByPriorityKey)
                .Select(Activator.CreateInstance)
                .Cast<ITransformConfig>()
                .ToList();

            foreach (var transformConfig in transformConfigs) {
                transformConfig.ConfigureTransformation(config);
            }
        }

        private static string GetOrderByPriorityKey(Type transformConfigType) {
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
