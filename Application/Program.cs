using Application.Configurations;
using Application.Middlewares;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using Shared.ExternalServices.Firebase;

// Static instances
FirebaseAppHelper.Init();

// Serilog for file logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Month,
        outputTemplate:
        "[{Level:w3}] {Timestamp:dd-MM-yyyy HH:mm:ss.fff zzz}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
    .CreateLogger();

// BUILDER
var builder = WebApplication.CreateBuilder(args);
{
    builder.Logging
        .ClearProviders()
        .AddConsole()
        .AddSerilog();

    builder.Services.AddRazorPages();
    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

    builder.Services
        .AddCors()
        .AddSession(options => options.IdleTimeout = TimeSpan.FromMinutes(60))
        .AddSwagger()
        .AddEndpointsApiExplorer()
        .AddSwaggerGenNewtonsoftSupport()
        .AddDependencies(builder.Configuration)
        .AddControllers()
        .AddMvcOptions(options => { options.SuppressAsyncSuffixInActionNames = true; })
        .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                // options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            }
        );
}

// APPLICATION
var app = builder.Build();
{
    app.Logger.LogInformation("Environment: {Env}", app.Environment.EnvironmentName);

    app.UseSession();
    app
        // .UseHttpsRedirection()
        .UseCors(x => x
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin())
        .UseMiddleware<HttpRequestLoggingMiddleware>()
        .UseMiddleware<JwtMiddleware>()
        .UseSwagger()
        .UseSwaggerUI();

    app.UseStaticFiles();
    app.UseRouting();
    app.MapRazorPages();

    app.MapControllers();
    app.Run();
}