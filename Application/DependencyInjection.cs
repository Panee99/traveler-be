using System.Reflection;
using Application.Configurations.Auth;
using Data;
using Mapster;
using MapsterMapper;
using Service.Implementations;
using Service.Interfaces;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        
        
        // Middleware
        services.AddScoped<JwtMiddleware>();
        
        // Services
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IVnPayRequestService, VnPayRequestService>();
        services.AddScoped<IVnPayResponseService, VnPayResponseService>();
        services.AddScoped<ICloudMessagingService, CloudMessagingService>();
        services.AddScoped<ITravelerService, TravelerService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ICloudStorageService, CloudStorageService>();
        
        return services;
    }
}