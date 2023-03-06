using Application.Configurations;
using Data;
using Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Utility.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSetting>(builder.Configuration.GetSection("AppSetting"));
// Add services to the container.
builder.Services.AddDbContext<TravelerDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    }
);
builder.Services.AddSwaggerGenNewtonsoftSupport();
builder.Services.AddCors();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();
builder.Services.AddDependenceInjection();

var app = builder.Build();

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
