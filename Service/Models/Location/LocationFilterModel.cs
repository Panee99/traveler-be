namespace Service.Models.Location;

public record LocationFilterModel
{
    public int Page;
    public int Size;
    public string? Name;
    public List<Guid>? Tags;
}