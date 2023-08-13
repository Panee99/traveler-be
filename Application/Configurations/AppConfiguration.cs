using Application.Middlewares;
using Application.Workers.Notification;
using Application.Workers.Weather;
using Data.EFCore;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Service.Channels.Notification;
using Service.Implementations;
using Service.Interfaces;
using Service.Settings;
using Shared.ExternalServices.Firebase;

namespace Application.Configurations;

public static class AppConfiguration
{
    public static IServiceCollection AddDependencies(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        // Mapper
        // services.AddScoped<IMapper, ServiceMapper>();

        // Settings
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<CloudStorageSettings>(configuration.GetSection("CloudStorageSettings"));
        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
        
        // DbContext
        services.AddDbContextPool<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddSingleton<IFirebaseClient>(new FirebaseClient(new FirebaseConfig
        {
            BasePath = configuration.GetSection("Firebase:RealtimeDatabase:BaseUrl").Value,
            AuthSecret = configuration.GetSection("Firebase:RealtimeDatabase:Secret").Value
        }));

        return services.AddDependencies();
    }

    private static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        // Workers
        services.AddSingleton<INotificationJobQueue>(new NotificationJobQueue());
        services.AddHostedService<NotificationWorker>();
        services.AddHostedService<UpdateWeatherWorker>();

        // Middleware
        services.AddScoped<HttpRequestLoggingMiddleware>();
        services.AddScoped<JwtMiddleware>();

        // Add all BaseService
        services.AddScoped<UnitOfWork>();
        services.Scan(scan => scan
            .FromAssemblyOf<BaseService>()
            .AddClasses(classes => classes.AssignableTo<BaseService>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Other services
        services.AddSingleton<ICloudStorageService, CloudStorageService>();
        services.AddSingleton<ICloudNotificationService, CloudNotificationService>();
        services.AddSingleton<ISmtpService, SmtpService>();
        services.AddSingleton(FirestoreHelper.GetInstance());

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