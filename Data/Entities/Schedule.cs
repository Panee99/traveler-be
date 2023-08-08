using Data.Enums;

namespace Data.Entities;

public class Schedule
{
    public Guid Id { get; set; }

    public Guid TourId { get; set; }

    public int Sequence { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Guid? ImageId { get; set; }

    public double? Longitude { get; set; }

    public double? Latitude { get; set; }

    public int DayNo { get; set; }

    public Vehicle? Vehicle { get; set; }

    //
    public Tour Tour { get; set; } = null!;

    public Attachment? Image { get; set; }
}