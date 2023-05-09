using Data.Enums;
using Service.Models.Attachment;
using Service.Models.Schedule;
using Service.Models.TourFlow;

namespace Service.Models.Tour;

public record TourViewModel
{
    public Guid Id;
    public string Title = null!;
    public double AdultPrice;
    public double ChildrenPrice;
    public double InfantPrice;
    public string Code = null!;
    public string Departure = null!;
    public string Destination = null!;
    public DateTime EndTime;
    public int MaxOccupancy;
    public DateTime StartTime;
    public string? Description;
    public string? ThumbnailUrl;
    public TourType Type;
    public TourStatus Status;
    public List<ScheduleViewModel> Schedules = new();
    public List<TourFlowViewModel> TourFlows = new();
    public List<AttachmentViewModel> Carousel = new();
}