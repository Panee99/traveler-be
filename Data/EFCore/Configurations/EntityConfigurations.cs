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

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(e => e.PaymentStatus).HasMaxLength(256);
            entity.Property(e => e.Timestamp).HasColumnType("datetime");

            entity.HasOne(d => d.Tour).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.TourId);

            entity.HasOne(d => d.Traveler).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.TravelerId);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(e => e.Timestamp).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(256);

            entity.HasOne(d => d.Booking).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.BookingId);
        });

        modelBuilder.Entity<TourFlow>();

        modelBuilder.Entity<TourCarousel>().HasKey(e => new { e.TourId, e.AttachmentId });

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
            entity.Property(e => e.Type).HasMaxLength(256);
            entity.Property(e => e.Status).HasMaxLength(256);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TourGroup>(entity => { entity.HasOne(e => e.Tour).WithMany(x => x.TourGroups); });

        modelBuilder.Entity<TourGuide>(entity =>
        {
            entity.ToTable("TourGuide");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Birthday).HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.Gender).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);
        });

        modelBuilder.Entity<Traveler>(entity =>
        {
            entity.ToTable("Traveler");
            entity.Property(e => e.Address).HasMaxLength(256);
            entity.Property(e => e.Birthday).HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.Gender).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);
        });

        modelBuilder.Entity<TravelerInTour>(entity =>
        {
            entity.ToTable("TravelerInTour");

            entity.HasOne(e => e.Tour).WithMany(tour => tour.TravelerInTours).HasForeignKey(e => e.TourId);

            entity.HasOne(e => e.Traveler).WithMany(traveler => traveler.TravelerInTours)
                .HasForeignKey(e => e.TravelerId);

            entity.HasOne(e => e.TourGroup).WithMany(tourGroup => tourGroup.TravelerInTours)
                .HasForeignKey(e => e.TourGroupId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VnPayRequest>(entity => { entity.HasKey(e => e.TxnRef); });

        modelBuilder.Entity<VnPayResponse>(entity =>
        {
            entity.HasKey(e => e.TxnRef);
            entity.HasOne(response => response.VnPayRequest)
                .WithOne()
                .HasForeignKey<VnPayResponse>(x => x.TxnRef);
        });

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

        modelBuilder.Entity<IncurredCost>(entity => { entity.Property(e => e.CreatedAt).HasColumnType("datetime"); });
    }
}