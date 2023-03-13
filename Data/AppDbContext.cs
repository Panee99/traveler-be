﻿using Data.Entities;
using Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; } = null!;
    public virtual DbSet<Attachment> Attachments { get; set; } = null!;
    public virtual DbSet<Booking> Bookings { get; set; } = null!;
    public virtual DbSet<BookingAppliedDiscount> BookingAppliedDiscounts { get; set; } = null!;
    public virtual DbSet<IncurredCost> IncurredCosts { get; set; } = null!;
    public virtual DbSet<Location> Locations { get; set; } = null!;
    public virtual DbSet<LocationTag> LocationTags { get; set; } = null!;
    public virtual DbSet<Manager> Managers { get; set; } = null!;
    public virtual DbSet<Tag> Tags { get; set; } = null!;
    public virtual DbSet<Tour> Tours { get; set; } = null!;
    public virtual DbSet<TourCarousel> TourCarousels { get; set; } = null!;
    public virtual DbSet<TourDiscount> TourDiscounts { get; set; } = null!;
    public virtual DbSet<TourGuide> TourGuides { get; set; } = null!;
    public virtual DbSet<Transaction> Transactions { get; set; } = null!;
    public virtual DbSet<Traveler> Travelers { get; set; } = null!;
    public virtual DbSet<Waypoint> Waypoints { get; set; } = null!;
    public virtual DbSet<VnPayRequest> VnPayRequests { get; set; } = null!;
    public virtual DbSet<VnPayResponse> VnPayResponses { get; set; } = null!;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<AccountStatus>().HaveConversion<string>();
        configurationBuilder.Properties<PaymentStatus>().HaveConversion<string>();
        configurationBuilder.Properties<Gender>().HaveConversion<string>();
        configurationBuilder.Properties<VnPayRequestStatus>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Phone).IsUnique();
            entity.Property(e => e.BankAccountNumber).HasMaxLength(256);
            entity.Property(e => e.BankName).HasMaxLength(256);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Password).HasMaxLength(256);
            entity.Property(e => e.Phone).HasMaxLength(256).IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(256);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Format).HasMaxLength(256);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PaymentStatus).HasMaxLength(256);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Tour).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Traveler).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.TravelerId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<BookingAppliedDiscount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(d => d.Booking).WithMany(p => p.BookingAppliedDiscounts)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.DiscountNavigation).WithMany(p => p.BookingAppliedDiscounts)
                .HasForeignKey(d => d.DiscountId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<IncurredCost>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.HasOne(d => d.TourGuide).WithMany(p => p.IncurredCosts)
                .HasForeignKey(d => d.TourGuideId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Tour).WithMany(p => p.IncurredCosts)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Address).HasMaxLength(256);
            entity.Property(e => e.Name).HasMaxLength(256);
        });

        modelBuilder.Entity<LocationTag>(entity =>
        {
            entity.HasKey(e => new { e.LocationId, e.TagId });
            entity.HasOne(d => d.Location).WithMany(p => p.LocationTags)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Tag).WithMany(p => p.LocationTags)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Manager>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Birthday).HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.Gender).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);

            entity.HasOne(d => d.Account).WithMany(p => p.Managers)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(256);
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(256);
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Type).HasMaxLength(256);
            entity.Property(e => e.Vehicle).HasMaxLength(256);
        });

        modelBuilder.Entity<TourCarousel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(d => d.Attachment).WithMany(p => p.TourCarousels)
                .HasForeignKey(d => d.AttachmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Tour).WithMany(p => p.TourCarousels)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TourDiscount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.T1discount).HasColumnName("T1Discount");
            entity.Property(e => e.T1discountPercent).HasColumnName("T1DiscountPercent");
            entity.Property(e => e.T1numberOfFirstTickets).HasColumnName("T1NumberOfFirstTickets");
            entity.Property(e => e.T2discount).HasColumnName("T2Discount");
            entity.Property(e => e.T2discountPercent).HasColumnName("T2DiscountPercent");
            entity.Property(e => e.T2numberOfTickets).HasColumnName("T2NumberOfTickets");

            entity.HasOne(d => d.Tour).WithMany(p => p.TourDiscounts)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TourGuide>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Birthday).HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.Gender).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);

            entity.HasOne(d => d.Account).WithMany(p => p.TourGuides)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(256);
            entity.Property(e => e.Type).HasMaxLength(256);

            entity.HasOne(d => d.Account).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Booking).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.BookingId);
        });

        modelBuilder.Entity<Traveler>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Address).HasMaxLength(256);
            entity.Property(e => e.Birthday).HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.Gender).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);

            entity.HasOne(d => d.Account).WithMany(p => p.Travelers)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Waypoint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ArrivalAt).HasColumnType("datetime");

            entity.HasOne(d => d.Location).WithMany(p => p.Waypoints)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Tour).WithMany(p => p.Waypoints)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<VnPayRequest>(entity => { entity.HasKey(e => e.TxnRef); });
        
        modelBuilder.Entity<VnPayResponse>(entity => { entity.HasKey(e => e.TxnRef); });
        modelBuilder.Entity<VnPayResponse>()
            .HasOne(response => response.VnPayRequest)
            .WithOne(request => request.VnPayResponse)
            .HasForeignKey<VnPayResponse>(x => x.TxnRef);
    }
}