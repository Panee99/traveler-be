using Data.Enums;

namespace Service.Models.Tag;

public record TagFilterModel
{
    public string? Name;

    public TagType? Type;
}