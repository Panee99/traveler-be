using Data.Enums;

namespace Service.Models.Activity;

public record ActivityCreateModel
(
    ActivityType Type,
    string Title,
    string? Description,
    Guid TourGroupId,
    DateTime StartAt
);