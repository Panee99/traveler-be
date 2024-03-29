﻿namespace Data.Entities;

public class TourImage
{
    public Guid TourId { get; set; }

    public Guid AttachmentId { get; set; }

    public virtual Tour Tour { get; set; } = null!;

    public virtual Attachment Attachment { get; set; } = null!;
}