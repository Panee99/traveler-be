namespace Data.Entities;

public class TourDiscount
{
    public Guid Id { get; set; }

    public Guid TourId { get; set; }

    public string? Description { get; set; }

    public int? T1numberOfFirstTickets { get; set; }

    public double? T1discount { get; set; }

    public double? T1discountPercent { get; set; }

    public int? T2numberOfTickets { get; set; }

    public double? T2discount { get; set; }

    public double? T2discountPercent { get; set; }

    public virtual ICollection<BookingAppliedDiscount> BookingAppliedDiscounts { get; } = new List<BookingAppliedDiscount>();

    public virtual Tour Tour { get; set; } = null!;
}
