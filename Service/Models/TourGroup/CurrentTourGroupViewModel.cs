﻿using Data.Enums;
using Service.Models.Trip;

namespace Service.Models.TourGroup;

public class CurrentTourGroupViewModel
{
    public Guid Id;
    public DateTime CreatedAt;
    public int TotalDays;
    public string GroupName = null!;
    public Guid? TourGuideId;
    public Guid TripId;
    public int TravelerCount;
    public Guid? CurrentScheduleId;
    public TourGroupStatus Status;
    public TripViewModel? Trip;
}