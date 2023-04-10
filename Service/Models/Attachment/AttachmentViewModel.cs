namespace Service.Models.Attachment;

public record AttachmentViewModel
{
    public Guid Id;
    public string ContentType = null!;
    public string? Url = null!;
}