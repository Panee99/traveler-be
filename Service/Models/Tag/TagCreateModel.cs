using Data.Enums;

namespace Service.Models.Tag;

public record TagCreateModel
(
    string Name,
    TagType Type
);