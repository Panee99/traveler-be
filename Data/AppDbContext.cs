using System.Reflection;
using Data.Configurations;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; } = null!;
    public virtual DbSet<Manager> Managers { get; set; } = null!;
    public virtual DbSet<Traveler> Travelers { get; set; } = null!;
    public virtual DbSet<TourGuide> TourGuides { get; set; } = null!;
    public virtual DbSet<Attachment> Attachments { get; set; } = null!;
    public virtual DbSet<LocationAttachment> LocationAttachments { get; set; } = null!;
    public virtual DbSet<Location> Locations { get; set; } = null!;
    public virtual DbSet<Tag> Tags { get; set; } = null!;
    public virtual DbSet<LocationTag> LocationTags { get; set; } = null!;
    public virtual DbSet<Tour> Tours { get; set; } = null!;
    public virtual DbSet<Waypoint> Waypoints { get; set; } = null!;
    public virtual DbSet<Booking> Bookings { get; set; } = null!;
    public virtual DbSet<BookingAppliedDiscount> BookingAppliedDiscounts { get; set; } = null!;
    public virtual DbSet<IncurredCost> IncurredCosts { get; set; } = null!;
    public virtual DbSet<TourDiscount> TourDiscounts { get; set; } = null!;
    public virtual DbSet<Transaction> Transactions { get; set; } = null!;
    public virtual DbSet<VnPayRequest> VnPayRequests { get; set; } = null!;
    public virtual DbSet<VnPayResponse> VnPayResponses { get; set; } = null!;

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => base.OnConfiguring(optionsBuilder);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.ConfigureEnums();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureEntities();

        // Seeds
        modelBuilder.Entity<Traveler>().HasData(FromSeed<Traveler>("travelers.json"));
        modelBuilder.Entity<TourGuide>().HasData(FromSeed<TourGuide>("tour-guides.json"));
        modelBuilder.Entity<Manager>().HasData(FromSeed<Manager>("managers.json"));
    }

    // PRIVATE
    private static List<T> FromSeed<T>(string fileName)
    {
        var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        var seedPath = Path.Combine(projectPath, "Seeds", fileName);
        return JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(seedPath))!;
    }
}