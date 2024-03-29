using Newtonsoft.Json;

namespace Data.Entities.Activities;

public class AttendanceItem
{
    public Guid Id { get; set; }

    [JsonIgnore] public Guid AttendanceActivityId { get; set; }

    public Guid UserId { get; set; }

    public bool Present { get; set; } = false;

    public string Reason { get; set; } = string.Empty;

    public DateTime? AttendanceAt { get; set; }

    public User User { get; set; } = null!;

    public DateTime? LastUpdateAt { get; set; }
}