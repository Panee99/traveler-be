using System;
using System.Collections.Generic;

namespace Data.Entities;

public partial class Attachment
{
    public Guid Id { get; set; }

    public string Url { get; set; } = null!;

    public string Format { get; set; } = null!;

    public virtual ICollection<TourCarousel> TourCarousels { get; } = new List<TourCarousel>();
}
