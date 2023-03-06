using System;
using System.Collections.Generic;

namespace Data.Entities;

public partial class Tag
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<LocationTag> LocationTags { get; } = new List<LocationTag>();
}
