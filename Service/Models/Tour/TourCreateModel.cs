using Data.Enums;
using Service.Models.Schedule;
using Service.Models.TourFlow;

namespace Service.Models.Tour;

public record TourCreateModel
(
    string Title,
    string Departure,
    string Destination,
    int MaxOccupancy,
    TourType Type,
    string Duration,
    string? Description,
    string? Policy,
    List<ScheduleCreateModel>? Schedules,
    List<TourFlowCreateModel>? TourFlows,
    List<Guid>? Carousel,
    Guid? ThumbnailId
);