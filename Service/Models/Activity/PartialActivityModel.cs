using Data.Entities.Activities;
using Data.Enums;

namespace Service.Models.Activity;

public class PartialActivityModel
{
    public ActivityType Type { get; set; }

    public AttendanceActivity? AttendanceActivity { get; set; }

    public CheckInActivity? CheckInActivity { get; set; }

    public IncurredCostActivity? IncurredCostActivity { get; set; }
}