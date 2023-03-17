using Data.Enums;

namespace Data.Entities;

public class Tag
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public TagType Type { get; set; }
}

