using Data.Enums;
using Service.Models.Schedule;

namespace Service.Models.Tour;

public record TourCreateModel
(
    string Title,
    string Departure,
    string Destination,
    TourType Type,
    string Duration,
    string? Description,
    string? Policy,
    List<ScheduleCreateModel>? Schedules,
    List<Guid>? Carousel,
    Guid? ThumbnailId
);