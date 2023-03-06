using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class TravelerDbContext : DbContext
{
    public TravelerDbContext(DbContextOptions<TravelerDbContext> options)
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

    public virtual DbSet<Traveller> Travellers { get; set; } = null!;

    public virtual DbSet<Waypoint> Waypoints { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3214EC0701BD200E");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Phone, "UQ__Account__5C7E359E20C068D0").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.BankAccountNumber).HasMaxLength(256);
            entity.Property(e => e.BankName).HasMaxLength(256);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Password).HasMaxLength(256);
            entity.Property(e => e.Phone)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(256);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attachme__3214EC079CA284B7");

            entity.ToTable("Attachment");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Format).HasMaxLength(256);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Booking__3214EC0721F8E6DB");

            entity.ToTable("Booking");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.PaymentStatus).HasMaxLength(256);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Tour).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Booking__TourId__4D94879B");

            entity.HasOne(d => d.Traveller).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.TravellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Booking__Travell__4E88ABD4");
        });

        modelBuilder.Entity<BookingAppliedDiscount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingA__3214EC07FCAA017C");

            entity.ToTable("BookingAppliedDiscount");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingAppliedDiscounts)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingAp__Booki__5165187F");

            entity.HasOne(d => d.DiscountNavigation).WithMany(p => p.BookingAppliedDiscounts)
                .HasForeignKey(d => d.DiscountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingAp__Disco__52593CB8");
        });

        modelBuilder.Entity<IncurredCost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Incurred__3214EC078C6863A1");

            entity.ToTable("IncurredCost");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.TourGuide).WithMany(p => p.IncurredCosts)
                .HasForeignKey(d => d.TourGuideId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__IncurredC__TourG__59FA5E80");

            entity.HasOne(d => d.Tour).WithMany(p => p.IncurredCosts)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__IncurredC__TourI__59063A47");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Location__3214EC078BE54A78");

            entity.ToTable("Location");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(256);
            entity.Property(e => e.Name).HasMaxLength(256);
        });

        modelBuilder.Entity<LocationTag>(entity =>
        {
            entity.HasKey(e => new { e.LocationId, e.TagId }).HasName("PK__Location__31A96B0D8EBCBF58");

            entity.ToTable("LocationTag");

            entity.HasOne(d => d.Location).WithMany(p => p.LocationTags)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LocationT__Locat__60A75C0F");

            entity.HasOne(d => d.Tag).WithMany(p => p.LocationTags)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LocationT__TagId__619B8048");
        });

        modelBuilder.Entity<Manager>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Manager__3214EC07999D2232");

            entity.ToTable("Manager");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Birthday).HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.Gender).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);

            entity.HasOne(d => d.Account).WithMany(p => p.Managers)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Manager__Account__3C69FB99");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tag__3214EC071A55CD06");

            entity.ToTable("Tag");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(256);
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tour__3214EC07069545E6");

            entity.ToTable("Tour");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code).HasMaxLength(256);
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Type).HasMaxLength(256);
            entity.Property(e => e.Vehicle).HasMaxLength(256);
        });

        modelBuilder.Entity<TourCarousel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourCaro__3214EC0793BADC64");

            entity.ToTable("TourCarousel");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Attachment).WithMany(p => p.TourCarousels)
                .HasForeignKey(d => d.AttachmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TourCarou__Attac__4AB81AF0");

            entity.HasOne(d => d.Tour).WithMany(p => p.TourCarousels)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TourCarou__TourI__49C3F6B7");
        });

        modelBuilder.Entity<TourDiscount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourDisc__3214EC070FF3B8D6");

            entity.ToTable("TourDiscount");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.T1discount).HasColumnName("T1Discount");
            entity.Property(e => e.T1discountPercent).HasColumnName("T1DiscountPercent");
            entity.Property(e => e.T1numberOfFirstTickets).HasColumnName("T1NumberOfFirstTickets");
            entity.Property(e => e.T2discount).HasColumnName("T2Discount");
            entity.Property(e => e.T2discountPercent).HasColumnName("T2DiscountPercent");
            entity.Property(e => e.T2numberOfTickets).HasColumnName("T2NumberOfTickets");

            entity.HasOne(d => d.Tour).WithMany(p => p.TourDiscounts)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TourDisco__TourI__46E78A0C");
        });

        modelBuilder.Entity<TourGuide>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourGuid__3214EC07731CD1CC");

            entity.ToTable("TourGuide");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Birthday).HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.Gender).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);

            entity.HasOne(d => d.Account).WithMany(p => p.TourGuides)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TourGuide__Accou__3F466844");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC07495EE5DB");

            entity.ToTable("Transaction");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(256);
            entity.Property(e => e.Type).HasMaxLength(256);

            entity.HasOne(d => d.Account).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacti__Accou__5535A963");

            entity.HasOne(d => d.Booking).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__Transacti__Booki__5629CD9C");
        });

        modelBuilder.Entity<Traveller>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Travelle__3214EC070C655034");

            entity.ToTable("Traveller");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(256);
            entity.Property(e => e.Birthday).HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.Gender).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);

            entity.HasOne(d => d.Account).WithMany(p => p.Travellers)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Traveller__Accou__4222D4EF");
        });

        modelBuilder.Entity<Waypoint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Waypoint__3214EC07FEB9928C");

            entity.ToTable("Waypoint");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ArrivalAt).HasColumnType("datetime");

            entity.HasOne(d => d.Location).WithMany(p => p.Waypoints)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Waypoint__Locati__656C112C");

            entity.HasOne(d => d.Tour).WithMany(p => p.Waypoints)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Waypoint__TourId__6477ECF3");
        });
    }
}