using Data.Enums;

namespace Service.Models.Activity;

public record ActivityUpdateModel
(
    ActivityStatus? Status,
    string? Title,
    string? Description,
    DateTime? StartAt
);