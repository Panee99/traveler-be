using Data.Enums;

namespace Data.Entities;

public class Booking
{
    public Guid Id { get; set; }

    public Guid TourVariantId { get; set; }

    public Guid TravelerId { get; set; }

    public virtual TourVariant TourVariant { get; set; } = null!;

    public virtual Traveler Traveler { get; set; } = null!;

    public int AdultQuantity { get; set; }

    public int ChildrenQuantity { get; set; }

    public int InfantQuantity { get; set; }

    public int AdultPrice { get; set; }

    public int ChildrenPrice { get; set; }

    public int InfantPrice { get; set; }

    public DateTime ExpireAt { get; set; }

    public DateTime Timestamp { get; set; }

    public BookingStatus Status { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    // public virtual ICollection<BookingAppliedDiscount> BookingAppliedDiscounts { get; set; } =
    //     new List<BookingAppliedDiscount>();
}