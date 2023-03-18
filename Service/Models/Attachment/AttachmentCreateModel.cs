namespace Service.Models.Attachment;

public record AttachmentCreateModel
(
    string ContentType,
    Stream Stream
);