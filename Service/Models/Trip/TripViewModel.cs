using Service.Models.Tour;

namespace Service.Models.Trip;

public record TripViewModel
{
    public Guid Id;
    public string Code = null!;
    public DateTime StartTime;
    public DateTime EndTime;
    public Guid TourId;
    public TourViewModel? Tour;
}