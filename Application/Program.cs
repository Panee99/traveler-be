using Application.Configurations;
using Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddSettings();
builder.Services.AddDependenceInjection();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddSwaggerGenNewtonsoftSupport();
builder.Services.AddSwagger();
builder.Services.AddCors();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    );

var app = builder.Build();
Console.WriteLine(app.Environment.EnvironmentName);

app.UseCors(x => x
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowAnyOrigin());

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseJwt();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();