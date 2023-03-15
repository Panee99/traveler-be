namespace Data.Entities;

public class LocationAttachment
{
    public Guid LocationId { get; set; }

    public Guid AttachmentId { get; set; }

    public virtual Location Location { get; set; } = null!;

    public virtual Attachment Attachment { get; set; } = null!;
}