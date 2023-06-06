using Service.Models.Attachment;
using Service.Models.Schedule;
using Service.Models.TourFlow;

namespace Service.Models.Tour;

public record TourDetailsViewModel : TourViewModel
{
    public List<ScheduleViewModel> Schedules = new();
    public List<TourFlowViewModel> TourFlows = new();
    public List<AttachmentViewModel> Carousel = new();
}