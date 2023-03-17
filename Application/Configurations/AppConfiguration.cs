using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Shared.Settings;

namespace Application.Configurations
{
    public static class AppConfiguration
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services,
            IConfiguration configuration)
        {
            // Settings
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
            services.Configure<VnPaySettings>(configuration.GetSection("VnPaySettings"));

            // DbContext
            services.AddDbContextPool<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
            return services.AddDependencies();
        }

        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
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
                    Type = SecuritySchemeType.ApiKey,
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
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
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            return services;
        }
    }
}