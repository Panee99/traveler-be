using Service.Models.Location;

namespace Service.Models.TourFlow;

public record TourFlowViewModel
{
    public DateTime ArrivalAt;
    public string? Description;
    public bool IsPrimary;
    public LocationViewModel Location = null!;
    public Guid TourId;
}