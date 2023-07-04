using Data.Entities.Activities;
using Data.Enums;

namespace Service.Models.Activity;

public class CreateActivityModel
{
    public ActivityType Type { get; set; }

    public AttendanceActivity? AttendanceActivity { get; set; }

    public CustomActivity? CustomActivity { get; set; }

    public NextDestinationActivity? NextDestinationActivity { get; set; }
}