using System.Reflection;
using Application.Configurations.Auth;
using Data.EFCore;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Service.Implementations;
using Shared.ExternalServices.VnPay;
using Shared.Settings;

namespace Application.Configurations;

public static class AppConfiguration
{
    public static IServiceCollection AddDependencies(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        // Mapper
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        // Settings
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<VnPaySettings>(configuration.GetSection("VnPaySettings"));
        services.Configure<CloudStorageSettings>(configuration.GetSection("CloudStorageSettings"));

        // DbContext
        services.AddDbContextPool<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // VnPay
        services.AddSingleton<VnPay>();

        return services.AddDependencies();
    }

    private static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        services.AddScoped<JwtMiddleware>();
        services.AddScoped<UnitOfWork>();

        // Inject Services
        services.Scan(scan => scan
            .FromAssemblyOf<BaseService>()
            .AddClasses(classes => classes.AssignableTo<BaseService>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();
            c.SwaggerDoc("v1",
                new OpenApiInfo
                {
                    Title = "Traveler Service APIs",
                    Version = "v1"
                });
            c.DescribeAllParametersInCamelCase();
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });
        });

        return services;
    }
}