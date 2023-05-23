using Data.Enums;
using Service.Models.Attachment;
using Service.Models.Schedule;
using Service.Models.TourFlow;

namespace Service.Models.Tour;

public record TourViewModel
{
    public Guid Id;
    public string Title = null!;
    public string Departure = null!;
    public string Destination = null!;
    public int MaxOccupancy;
    public string? Description;
    public string? Policy;
    public string? ThumbnailUrl;
    public TourType Type;
    public TourStatus Status;
    public List<ScheduleViewModel> Schedules = new();
    public List<TourFlowViewModel> TourFlows = new();
    public List<AttachmentViewModel> Carousel = new();
}