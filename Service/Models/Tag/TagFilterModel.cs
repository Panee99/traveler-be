using Data.Enums;

namespace Service.Models.Tag;

public record TagFilterModel
(
    string? Name,
    TagType? Type
);