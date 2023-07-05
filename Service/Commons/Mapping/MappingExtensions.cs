using System.Diagnostics.CodeAnalysis;
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
    
    /// <summary>
    /// Custom AdaptIgnoreNull (not using Mapster)
    /// Using this method when adapt model at runtime (type dynamic)    
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    public static void CustomAdaptIgnoreNull<TSource, TDestination>(this TSource source, TDestination destination)
    {
        if (source == null) return;
        destination ??= Activator.CreateInstance<TDestination>();
        
        // for each dataModel property, if it is not null, then update the activity property
        foreach (var property in source.GetType().GetProperties())
        {
            var value = property.GetValue(source);
            if (value == null) continue;
            
            // bind the value to the activity
            var destinationProperty = destination!.GetType().GetProperty(property.Name);
            destinationProperty?.SetValue(destination, value);
        }
    }
}