using Data.Enums;

namespace Data.Entities;

public class Schedule
{
    public Guid Id { get; set; }

    public int Sequence { get; set; }

    public virtual Tour Tour { get; set; } = null!;

    public string Description { get; set; } = null!;

    public double? Longitude { get; set; }

    public double? Latitude { get; set; }

    public int DayNo { get; set; }

    public Vehicle? Vehicle { get; set; }

    //
    public Guid TourId { get; set; }
}