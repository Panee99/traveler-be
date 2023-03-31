using Data.Enums;

namespace Service.Models.Tag;

public record TagViewModel
(
    Guid Id,
    string Name,
    TagType Type
);