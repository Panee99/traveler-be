namespace Service.Models.Attachment;

public record AttachmentViewModel
{
    public string ContentType = null!;
    public Guid Id;
    public string? Url = null!;
}