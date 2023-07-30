namespace Data.Entities;

public class Attachment
{
    public Guid Id { get; set; }

    public string ContentType { get; set; } = null!;

    public string Extension { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    //
    public string FileName => $"{Id}.{Extension}";
}