using Data.Entities;
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
            entity.Property(e => e.Password).HasMaxLength(256);
            entity.Property(e => e.Phone).HasMaxLength(256).IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(256);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.Property(e => e.ContentType).HasMaxLength(256);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(e => e.Status).HasMaxLength(256);
            entity.Property(e => e.Timestamp).HasColumnType("datetime");

            entity.HasOne(d => d.Trip).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.TripId);

            entity.HasOne(d => d.Traveler).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.TravelerId);
        });

        modelBuilder.Entity<Feedback>();

        modelBuilder.Entity<IncurredCost>(
            entity => { entity.Property(e => e.CreatedAt).HasColumnType("datetime"); });

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.ToTable("Admin");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.Gender).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);
        });

        modelBuilder.Entity<Notification>();

        modelBuilder.Entity<Passenger>();

        modelBuilder.Entity<Schedule>();

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.Departure).HasMaxLength(256);
            entity.Property(e => e.Destination).HasMaxLength(256);
            entity.Property(e => e.Type).HasMaxLength(256);
            entity.Property(e => e.Status).HasMaxLength(256);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(256);
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(256);
        });

        modelBuilder.Entity<TourGroup>(entity => { entity.HasOne(e => e.Trip).WithMany(x => x.TourGroups); });

        modelBuilder.Entity<TourGuide>(entity =>
        {
            entity.ToTable("TourGuide");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.Gender).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);
        });

        modelBuilder.Entity<TourImage>().HasKey(e => new { e.TourId, e.AttachmentId });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(e => e.Timestamp).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(256);

            entity.HasOne(d => d.Booking).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.BookingId);
        });

        modelBuilder.Entity<Traveler>(entity =>
        {
            entity.ToTable("Traveler");
            entity.Property(e => e.Address).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.Gender).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);
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

        modelBuilder.Entity<VnPayResponse>(entity =>
        {
            entity.HasKey(e => e.TransactionId);
            entity.HasOne(response => response.Transaction)
                .WithOne()
                .HasForeignKey<VnPayResponse>(x => x.TransactionId);
        });

        modelBuilder.Entity<AttendanceEvent>(entity =>
        {
            entity.HasOne(evt => evt.TourGroup)
                .WithMany(group => group.AttendanceEvents)
                .HasForeignKey(evt => evt.TourGroupId);
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasOne(att => att.AttendanceEvent)
                .WithMany(evt => evt.Attendances)
                .HasForeignKey(e => e.AttendanceEventId);
        });

        modelBuilder.Entity<FcmToken>(entity =>
        {
            entity.HasOne(token => token.User)
                .WithMany(user => user.FcmTokens)
                .HasForeignKey(e => e.UserId);
        });
    }
}