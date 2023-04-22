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
        modelBuilder.Entity<Traveler>().HasSeedData("travelers.json");
        modelBuilder.Entity<TourGuide>().HasSeedData("tour-guides.json");
        modelBuilder.Entity<Manager>().HasSeedData("managers.json");
        modelBuilder.Entity<Location>().HasSeedData("locations.json");
        modelBuilder.Entity<TourGroup>().HasSeedData("tour-groups.json");
        modelBuilder.Entity<Tour>().HasSeedData("tours.json");
        modelBuilder.Entity<Ticket>().HasSeedData("tickets.json");
        modelBuilder.Entity<Attachment>().HasSeedData("attachments.json");
        // modelBuilder.Entity<TravelerInTourGroup>().HasSeedData("traveler-in-tour-group.json");
    }
}

public static class DbContextExtensions
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