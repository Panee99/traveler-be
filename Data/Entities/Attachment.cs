namespace Data.Entities;

public class Attachment
{
    public Guid Id { get; set; }

    public string ContentType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}