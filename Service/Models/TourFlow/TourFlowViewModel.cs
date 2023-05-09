namespace Service.Models.TourFlow;

public class TourFlowViewModel
{
    public Guid Id;
    public float Longitude;
    public float Latitude;
    public DateTime ArrivalTime;
    public string? Description = null!;
}