using Data.Enums;

namespace Service.Models.Activity;

public record ActivityViewModel
{
    public Guid Id;
    public ActivityType Type;
    public string Title = null!;
    public string? Description;
    public Guid TourGroupId;
    public DateTime StartAt;
    public DateTime CreatedAt;
}