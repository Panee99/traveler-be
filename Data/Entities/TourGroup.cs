using Data.Entities.Activities;
using Data.Enums;

namespace Data.Entities;

public class TourGroup
{
    public Guid Id { get; set; }

    public Guid TripId { get; set; }

    public Guid? TourGuideId { get; set; }

    public Guid? CurrentScheduleId { get; set; }

    public string GroupName { get; set; } = null!;

    public int GroupNo { get; set; }

    public DateTime CreatedAt { get; set; }

    public Trip Trip { get; set; } = null!;

    public TourGuide? TourGuide { get; set; } = null!;

    public Schedule? CurrentSchedule { get; set; }

    public TourGroupStatus Status { get; set; }

    public virtual ICollection<Traveler> Travelers { get; set; } = new List<Traveler>();

    public virtual ICollection<AttendanceActivity> AttendanceActivities { get; set; } = new List<AttendanceActivity>();
}