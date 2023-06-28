using Data.Enums;

namespace Data.Entities;

public class Activity
{
    public Guid Id { get; set; }

    public ActivityType Type { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime StartAt { get; set; }

    public Guid TourGroupId { get; set; }

    public DateTime CreatedAt { get; set; }

    public TourGroup TourGroup { get; set; } = null!;
}