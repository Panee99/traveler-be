using System.Reflection;
using Data.EFCore.Configurations;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace Data.EFCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.ConfigureEnums();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureEntities();

        // Seeds
        modelBuilder.Entity<Attachment>().HasSeedData("attachments.json");
        modelBuilder.Entity<Admin>().HasSeedData("admins.json");
        modelBuilder.Entity<Schedule>().HasSeedData("schedules.json");
        modelBuilder.Entity<TourGroup>().HasSeedData("tour-groups.json");
        modelBuilder.Entity<TourGuide>().HasSeedData("tour-guides.json");
        modelBuilder.Entity<TourImage>().HasSeedData("tour-images.json");
        modelBuilder.Entity<TourVariant>().HasSeedData("tour-variants.json");
        modelBuilder.Entity<Tour>().HasSeedData("tours.json");
        modelBuilder.Entity<Traveler>().HasSeedData("travelers.json");
        modelBuilder.Entity<TravelerInTourGroup>().HasSeedData("travelers-in-tour-groups.json");
        modelBuilder.Entity<Notification>().HasSeedData("notifications.json");
    }
}

internal static class DbContextExtensions
{
    public static void HasSeedData<TEntity>(this EntityTypeBuilder<TEntity> builder, string fileName)
        where TEntity : class
    {
        builder.HasData(_fromSeed<TEntity>(fileName));
    }

    private static IEnumerable<T> _fromSeed<T>(string fileName)
    {
        var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        var seedPath = Path.Combine(projectPath, "EFCore", "Seeds", fileName);
        return JsonConvert.DeserializeObject<IEnumerable<T>>(File.ReadAllText(seedPath))!;
    }
}