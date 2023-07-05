using Data.Enums;

namespace Service.Models.Activity;

public class ActivityViewModel
{
    public ActivityType Type { get; set; }

    public dynamic Data { get; set; }

    public DateTime CreatedAt { get; set; }
}