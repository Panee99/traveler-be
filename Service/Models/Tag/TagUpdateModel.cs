using Data.Enums;

namespace Service.Models.Tag;

public record TagUpdateModel
{
    public string Name = "";

    public TagType Type;
}