using System;
using System.Collections.Generic;

namespace Data.Entities;

public partial class Waypoint
{
    public Guid Id { get; set; }

    public Guid TourId { get; set; }

    public Guid LocationId { get; set; }

    public DateTime ArrivalAt { get; set; }

    public bool IsPrimary { get; set; }

    public virtual Location Location { get; set; } = null!;

    public virtual Tour Tour { get; set; } = null!;
}
