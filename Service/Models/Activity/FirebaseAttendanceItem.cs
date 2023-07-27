using System.Collections;
using Data.Entities.Activities;

namespace Service.Models.Activity;

public class FirebaseAttendanceItem
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }

    public string AvatarUrl { get; set; }

    public String Name { get; set; }

    public bool Present { get; set; } = false;

    public string Reason { get; set; } = string.Empty;

    public DateTime? AttendanceAt { get; set; }
    
    public DateTime? LastUpdateAt { get; set; }
}

