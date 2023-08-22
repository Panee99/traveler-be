using Data.Entities;
using Data.Entities.Activities;
using Microsoft.EntityFrameworkCore;

namespace Data.EFCore.Configurations;

public static class EntityConfigurations
{
    public static void ConfigureEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Role);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Phone).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Phone).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Email).HasMaxLength(128);
            entity.Property(e => e.Password).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(64);
            entity.Property(e => e.LastName).HasMaxLength(64);
            entity.Property(e => e.Gender).HasMaxLength(64);
            entity.Property(e => e.Role).HasMaxLength(64);
            entity.Property(e => e.Status).HasMaxLength(64);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.Property(e => e.ContentType).HasMaxLength(256);
            entity.Property(e => e.Extension).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Manager>(entity =>
        {
            entity.ToTable("Manager");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.Gender).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(e => e.Type).HasMaxLength(128);
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.Payload).HasMaxLength(1024);
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(1024);
            entity.Property(e => e.Vehicle).HasMaxLength(64);
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.Title).HasMaxLength(512);
            entity.Property(e => e.Departure).HasMaxLength(256);
            entity.Property(e => e.Destination).HasMaxLength(256);
            entity.Property(e => e.Type).HasMaxLength(64);
            entity.Property(e => e.Duration).HasMaxLength(128);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(64);
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.HasOne(e => e.CreatedBy).WithMany()
                .HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<TourGroup>(entity =>
        {
            entity.HasOne(e => e.Trip).WithMany(x => x.TourGroups);
            entity.Property(e => e.GroupName).HasMaxLength(512);
            entity.Property(e => e.Status).HasMaxLength(64);
            entity.HasMany(x => x.AttendanceActivities).WithOne(x => x.TourGroup).HasForeignKey(x => x.TourGroupId);
        });

        modelBuilder.Entity<TourGuide>(entity =>
        {
            entity.ToTable("TourGuide");
            entity.Property(e => e.FirstContactNumber).HasMaxLength(20);
            entity.Property(e => e.SecondContactNumber).HasMaxLength(20);
        });

        modelBuilder.Entity<TourImage>().HasKey(e => new { e.TourId, e.AttachmentId });

        modelBuilder.Entity<Traveler>(entity =>
        {
            entity.ToTable("Traveler");
            entity.Property(e => e.Address).HasMaxLength(256);
        });

        modelBuilder.Entity<Traveler>()
            .HasMany(traveler => traveler.TourGroups)
            .WithMany(group => group.Travelers)
            .UsingEntity<TravelerInTourGroup>(
                builder => builder.HasOne(tig => tig.TourGroup).WithMany().HasForeignKey(tig => tig.TourGroupId),
                builder => builder.HasOne(tig => tig.Traveler).WithMany().HasForeignKey(sc => sc.TravelerId),
                builder =>
                {
                    builder.ToTable("TravelerInTourGroup");
                    builder.HasKey(sc => new { sc.TravelerId, sc.TourGroupId });
                }
            );

        modelBuilder.Entity<FcmToken>(entity =>
        {
            entity.HasOne(token => token.User)
                .WithMany(user => user.FcmTokens)
                .HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<AttendanceActivity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(256);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.Note).IsRequired().HasDefaultValue(string.Empty).HasMaxLength(512);
            entity.Property(x => x.TourGroupId).IsRequired();
            entity.Property(x => x.IsDeleted).HasDefaultValue(false);
            entity.Property(x => x.IsOpen).HasDefaultValue(true);
        });

        modelBuilder.Entity<AttendanceItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Present).IsRequired();
            entity.Property(x => x.Reason).IsRequired().HasDefaultValue(string.Empty).HasMaxLength(256);
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<CheckInActivity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(256);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.Note).IsRequired().HasDefaultValue(string.Empty).HasMaxLength(512);
            ;
            entity.Property(x => x.TourGroupId).IsRequired();
            entity.Property(x => x.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<IncurredCostActivity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(256);
            entity.Property(x => x.Currency).HasMaxLength(10);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.Note).IsRequired().HasDefaultValue(string.Empty).HasMaxLength(512);
            entity.Property(x => x.TourGroupId).IsRequired();
            entity.Property(x => x.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<WeatherAlert>(alert =>
        {
            alert.HasOne(e => e.Trip).WithMany(trip => trip.WeatherAlerts)
                .HasForeignKey(e => e.TripId);
        });
    }
}