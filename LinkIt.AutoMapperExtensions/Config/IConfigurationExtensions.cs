using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace LinkIt.AutoMapperExtensions.Config {
    public static class IConfigurationExtensions {

        public static void ApplyLoadLinkProtocolConfigs(this IConfiguration config, IEnumerable<Assembly> assemblies) {
            if (config == null) { throw new ArgumentNullException("config"); }
            if (assemblies == null) { throw new ArgumentNullException("assemblies"); }

            var transformConfigTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.GetInterface("LinkIt.AutoMapperExtensions.Config.ITransformConfig") != null)
                .ToList();

            var transformConfigs = transformConfigTypes
                .Select(Activator.CreateInstance)
                .Cast<ITransformConfig>()
                .ToList();

            foreach (var transformConfig in transformConfigs) {
                transformConfig.ConfigureTransformation(config);
            }
        }
    }
}
