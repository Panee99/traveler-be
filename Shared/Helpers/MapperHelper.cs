using Mapster;

namespace Shared.Helpers;

public class MapperHelper
{
    public static TypeAdapterConfig IgnoreNullConfig<TSource, TDestination>()
    {
        return TypeAdapterConfig<TSource, TDestination>
            .NewConfig()
            .IgnoreNullValues(true)
            .Config;
    }
}