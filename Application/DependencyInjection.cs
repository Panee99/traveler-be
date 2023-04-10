using Application.Configurations.Auth;
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
        services.AddScoped<IVnPayRequestService, VnPayRequestService>();
        services.AddScoped<IVnPayResponseService, VnPayResponseService>();
        services.AddScoped<ICloudMessagingService, CloudMessagingService>();
        services.AddScoped<ITravelerService, TravelerService>();
        services.AddScoped<ICloudStorageService, CloudStorageService>();
        services.AddScoped<ITourService, TourService>();
        services.AddScoped<ITourGroupService, TourGroupService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IAttachmentService, AttachmentService>();

        return services;
    }
}