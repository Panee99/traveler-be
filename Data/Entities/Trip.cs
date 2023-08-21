namespace Data.Entities;

public class Trip : BaseEntity
{
    public Guid Id { get; set; }

    public Guid TourId { get; set; }

    public Tour Tour { get; set; } = null!;

    public string Code { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public virtual ICollection<TourGroup> TourGroups { get; set; } = new List<TourGroup>();

    public virtual ICollection<WeatherAlert> WeatherAlerts { get; set; } = new List<WeatherAlert>();
}