using Data.Enums;

namespace Service.Models.Schedule;

public record ScheduleViewModel
{
    public Guid Id;
    public int Sequence;
    public string Description = null!;
    public double? Longitude;
    public double? Latitude;
    public Vehicle? Vehicle;
}