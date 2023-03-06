using System;
using System.Collections.Generic;

namespace Data.Entities;

public partial class TourCarousel
{
    public Guid Id { get; set; }

    public Guid TourId { get; set; }

    public Guid AttachmentId { get; set; }

    public virtual Attachment Attachment { get; set; } = null!;

    public virtual Tour Tour { get; set; } = null!;
}
