namespace Service.Models.TourFlow;

public class TourFlowViewModel
{
    public Guid Id;
    public double Longitude;
    public double Latitude;
    public DateTime ArrivalTime;
    public string? Description = null!;
}