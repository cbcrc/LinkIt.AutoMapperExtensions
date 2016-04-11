using AutoMapper;

namespace LinkIt.AutoMapperExtensions.Config
{
    public interface ITransformConfig
    {
        void ConfigureTransformation(IConfiguration config);
    }
}