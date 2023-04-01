using Service.Models.Location;

namespace Service.Models.TourFlow;

public record TourFlowViewModel
{
    public Guid TourId;
    public LocationViewModel Location = null!;
    public DateTime ArrivalAt;
    public bool IsPrimary;
    public string? Description;
}