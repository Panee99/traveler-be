namespace Service.Models.Location;

public record LocationFilterModel
{
    public int Page;
    public int Size;
    public List<Guid>? Tags;
}