namespace Data.Entities;

public class BookingAppliedDiscount
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Guid DiscountId { get; set; }

    public double Discount { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual TourDiscount DiscountNavigation { get; set; } = null!;
}
