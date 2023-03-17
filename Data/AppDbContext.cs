using Data.Configurations;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // public virtual DbSet<Account> Accounts { get; set; } = null!;
    public virtual DbSet<Attachment> Attachments { get; set; } = null!;
    public virtual DbSet<Booking> Bookings { get; set; } = null!;
    public virtual DbSet<BookingAppliedDiscount> BookingAppliedDiscounts { get; set; } = null!;
    public virtual DbSet<IncurredCost> IncurredCosts { get; set; } = null!;
    public virtual DbSet<Location> Locations { get; set; } = null!;
    public virtual DbSet<LocationTag> LocationTags { get; set; } = null!;
    public virtual DbSet<Manager> Managers { get; set; } = null!;
    public virtual DbSet<Tag> Tags { get; set; } = null!;
    public virtual DbSet<Tour> Tours { get; set; } = null!;
    public virtual DbSet<TourDiscount> TourDiscounts { get; set; } = null!;
    public virtual DbSet<TourGuide> TourGuides { get; set; } = null!;
    public virtual DbSet<Transaction> Transactions { get; set; } = null!;
    public virtual DbSet<Traveler> Travelers { get; set; } = null!;
    public virtual DbSet<Waypoint> Waypoints { get; set; } = null!;
    public virtual DbSet<VnPayRequest> VnPayRequests { get; set; } = null!;
    public virtual DbSet<VnPayResponse> VnPayResponses { get; set; } = null!;

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => base.OnConfiguring(optionsBuilder);
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureEntities();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.ConfigureEnums();
    }
}