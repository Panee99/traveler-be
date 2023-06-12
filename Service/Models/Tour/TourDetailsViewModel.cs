using Service.Models.Attachment;
using Service.Models.Schedule;

namespace Service.Models.Tour;

public record TourDetailsViewModel : TourViewModel
{
    public List<ScheduleViewModel> Schedules = new();
    public List<AttachmentViewModel> Carousel = new();
}