namespace Data.Entities;

public class Tag
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<LocationTag> LocationTags { get; } = new List<LocationTag>();
}
