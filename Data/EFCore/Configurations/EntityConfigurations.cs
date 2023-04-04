using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.EFCore.Configurations;

public static class EntityConfigurations
{
    public static void ConfigureEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Account");
            entity.HasIndex(e => e.Role);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Phone).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.Password).HasMaxLength(256);
            entity.Property(e => e.BankAccountNumber).HasMaxLength(256);
            entity.Property(e => e.BankName).HasMaxLength(256);
            entity.Property(e => e.Phone).HasMaxLength(256).IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(256);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.Property(e => e.ContentType).HasMaxLength(256);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Ticket>();

        // modelBuilder.Entity<Booking>(entity =>
        // {
        //     entity.Property(e => e.PaymentStatus).HasMaxLength(256);
        //     entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        //     entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        //
        //     entity.HasOne(d => d.Tour).WithMany(p => p.Bookings)
        //         .HasForeignKey(d => d.TourId)
        //         .OnDelete(DeleteBehavior.ClientSetNull);
        //
        //     entity.HasOne(d => d.Traveler).WithMany(p => p.Bookings)
        //         .HasForeignKey(d => d.TravelerId)
        //         .OnDelete(DeleteBehavior.ClientSetNull);
        // });

        // modelBuilder.Entity<BookingAppliedDiscount>(entity =>
        // {
        //     entity.HasOne(d => d.Booking).WithMany(p => p.BookingAppliedDiscounts)
        //         .HasForeignKey(d => d.BookingId)
        //         .OnDelete(DeleteBehavior.ClientSetNull);
        //
        //     entity.HasOne(d => d.DiscountNavigation).WithMany(p => p.BookingAppliedDiscounts)
        //         .HasForeignKey(d => d.DiscountId)
        //         .OnDelete(DeleteBehavior.ClientSetNull);
        // });

        // modelBuilder.Entity<IncurredCost>(entity =>
        // {
        //     entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        //     entity.HasOne(d => d.TourGuide).WithMany(p => p.IncurredCosts)
        //         .HasForeignKey(d => d.TourGuideId)
        //         .OnDelete(DeleteBehavior.ClientSetNull);
        // });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.Address).HasMaxLength(256);
            entity.Property(e => e.Country).HasMaxLength(256);
            entity.Property(e => e.City).HasMaxLength(256);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime2");
        });

        modelBuilder.Entity<LocationAttachment>().HasKey(e => new { e.LocationId, e.AttachmentId });

        modelBuilder.Entity<Manager>(entity =>
        {
            entity.ToTable("Manager");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Birthday).HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.Gender).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.Code).HasMaxLength(256);
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Departure).HasMaxLength(256);
            entity.Property(e => e.Destination).HasMaxLength(256);
            entity.Property(e => e.Vehicle).HasMaxLength(256);
            entity.Property(e => e.Type).HasMaxLength(256);
            entity.Property(e => e.Status).HasMaxLength(256);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime2");
        });

        modelBuilder.Entity<TourGroup>(entity => { entity.HasOne(e => e.Tour).WithMany(x => x.TourGroups); });

        // modelBuilder.Entity<TourDiscount>(entity =>
        // {
        //     entity.Property(e => e.T1discount).HasColumnName("T1Discount");
        //     entity.Property(e => e.T1discountPercent).HasColumnName("T1DiscountPercent");
        //     entity.Property(e => e.T1numberOfFirstTickets).HasColumnName("T1NumberOfFirstTickets");
        //     entity.Property(e => e.T2discount).HasColumnName("T2Discount");
        //     entity.Property(e => e.T2discountPercent).HasColumnName("T2DiscountPercent");
        //     entity.Property(e => e.T2numberOfTickets).HasColumnName("T2NumberOfTickets");
        //
        //     entity.HasOne(d => d.Tour).WithMany(p => p.TourDiscounts)
        //         .HasForeignKey(d => d.TourId)
        //         .OnDelete(DeleteBehavior.ClientSetNull);
        // });

        modelBuilder.Entity<TourGuide>(entity =>
        {
            entity.ToTable("TourGuide");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Birthday).HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.Gender).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);
        });

        // modelBuilder.Entity<Transaction>(entity =>
        // {
        //     entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        //     entity.Property(e => e.Description).IsUnicode(false);
        //     entity.Property(e => e.Status).HasMaxLength(256);
        //     entity.Property(e => e.Type).HasMaxLength(256);
        //
        //     entity.HasOne(d => d.Account).WithMany(p => p.Transactions)
        //         .HasForeignKey(d => d.AccountId)
        //         .OnDelete(DeleteBehavior.ClientSetNull);
        //
        //     entity.HasOne(d => d.Booking).WithMany(p => p.Transactions)
        //         .HasForeignKey(d => d.BookingId);
        // });

        modelBuilder.Entity<Traveler>(entity =>
        {
            entity.ToTable("Traveler");
            entity.Property(e => e.Address).HasMaxLength(256);
            entity.Property(e => e.Birthday).HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.Gender).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);
        });

        modelBuilder.Entity<TravelerInTourGroup>(entity => { entity.ToTable("TravelerInTourGroup"); });

        modelBuilder.Entity<TourFlow>(entity =>
        {
            entity.HasKey(e => new { e.LocationId, e.TourId });
            entity.Property(e => e.ArrivalAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<VnPayRequest>(entity => { entity.HasKey(e => e.TxnRef); });

        modelBuilder.Entity<VnPayResponse>(entity => { entity.HasKey(e => e.TxnRef); });
        modelBuilder.Entity<VnPayResponse>()
            .HasOne(response => response.VnPayRequest)
            .WithOne(request => request.VnPayResponse)
            .HasForeignKey<VnPayResponse>(x => x.TxnRef);
    }
}