using Data.Enums;

namespace Data.Entities;

public class Booking
{
    public Guid Id { get; set; }

    public Guid TourId { get; set; }

    public Guid TravelerId { get; set; }

    public int AdultQuantity { get; set; }

    public int ChildQuantity { get; set; }

    public int InfantQuantity { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public virtual ICollection<BookingAppliedDiscount> BookingAppliedDiscounts { get; set; } =
        new List<BookingAppliedDiscount>();

    public virtual Tour Tour { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual Traveler Traveler { get; set; } = null!;
}