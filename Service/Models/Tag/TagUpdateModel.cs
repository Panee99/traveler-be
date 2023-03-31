using Data.Enums;

namespace Service.Models.Tag;

public record TagUpdateModel
(
    string? Name,
    TagType? Type
);