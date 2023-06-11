using Mapster;

namespace Service.Commons.Mapping;

public static class MappingExtensions
{
    private static TypeAdapterConfig IgnoreNullConfig<TSource, TDestination>()
    {
        return TypeAdapterConfig<TSource, TDestination>
            .NewConfig()
            .IgnoreNullValues(true)
            .Config;
    }

    public static TDestination AdaptIgnoreNull<TSource, TDestination>(this TSource source, TDestination destination)
    {
        return source.Adapt(destination, IgnoreNullConfig<TSource, TDestination>());
    }

    public static TDestination AdaptIgnoreNull<TSource, TDestination>(this TSource source)
    {
        if (source is null) throw new Exception("Adapt source can not be null");
        return source.Adapt<TDestination>(IgnoreNullConfig<TSource, TDestination>());
    }
}