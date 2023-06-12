using Data.Enums;
using Service.Models.Schedule;

namespace Service.Models.Tour;

public record TourUpdateModel
(
    string? Title,
    string? Departure,
    string? Destination,
    int? MaxOccupancy,
    TourType? Type,
    string? Duration,
    string? Description,
    string? Policy,
    TourStatus? Status,
    List<ScheduleCreateModel>? Schedules,
    List<Guid>? Carousel,
    Guid? ThumbnailId
);