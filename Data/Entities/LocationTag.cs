using System;
using System.Collections.Generic;

namespace Data.Entities;

public partial class LocationTag
{
    public Guid LocationId { get; set; }

    public Guid TagId { get; set; }

    public string? Description { get; set; }

    public virtual Location Location { get; set; } = null!;

    public virtual Tag Tag { get; set; } = null!;
}
