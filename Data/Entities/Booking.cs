using System;
using System.Collections.Generic;

namespace Data.Entities;

public partial class Booking
{
    public Guid Id { get; set; }

    public Guid TourId { get; set; }

    public Guid TravellerId { get; set; }

    public int AdultQuantity { get; set; }

    public int ChildQuantity { get; set; }

    public int InfantQuantity { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public virtual ICollection<BookingAppliedDiscount> BookingAppliedDiscounts { get; } = new List<BookingAppliedDiscount>();

    public virtual Tour Tour { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; } = new List<Transaction>();

    public virtual Traveller Traveller { get; set; } = null!;
}
