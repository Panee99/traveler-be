﻿using Application.Configurations.Auth;
using Data.EFCore;
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
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IVnPayRequestService, VnPayRequestService>();
        services.AddScoped<IVnPayResponseService, VnPayResponseService>();
        services.AddScoped<ICloudMessagingService, CloudMessagingService>();
        services.AddScoped<ITravelerService, TravelerService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ICloudStorageService, CloudStorageService>();
        services.AddScoped<ITourService, TourService>();

        return services;
    }
}