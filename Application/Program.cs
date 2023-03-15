using Application.Configurations;
using Application.Configurations.Middleware;
using Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using Shared.Firebase;

// Serilog for file logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.File(path: "logs/log-.txt", rollingInterval: RollingInterval.Month,
        outputTemplate: "[{Level:w3}] {Timestamp:dd-MM-yyyy HH:mm:ss.fff zzz}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
    .CreateLogger();

// Init Firebase
FirebaseAppInitializer.Init();

// BUILDER
var builder = WebApplication.CreateBuilder(args);
{
    // Add Serilog
    builder.Logging
        .ClearProviders()
        .AddConsole()
        .AddSerilog();

    // Add services to the container.
    builder.AddSettings();
    builder.Services.AddDependenceInjection();

    builder.Services.AddDbContextPool<AppDbContext>(optionsBuilder =>
        optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    
    builder.Services.AddCors();
    builder.Services.AddSwagger();
    builder.Services.AddSwaggerGenNewtonsoftSupport();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    );
}

// APPLICATION
var app = builder.Build();
{
    Console.WriteLine(app.Environment.EnvironmentName);
    // Configure the HTTP request pipeline.
    app.UseCors(x => x
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin())
        .UseHttpsRedirection()
        .UseSwagger()
        .UseSwaggerUI()
        .UseAuthentication()
        .UseAuthorization()
        .UseMiddleware<JwtMiddleware>();
        
    app.MapControllers();
    app.Run();
}