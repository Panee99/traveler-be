﻿using Service.Models.TourVariant;

namespace Service.Models.TourGroup;

public class TourGroupViewModel
{
    public Guid Id;
    public Guid TourVariantId;
    public DateTime CreatedAt;
    public string GroupName = null!;
    public Guid? TourGuideId;
    public int MaxOccupancy;
    public TourVariantViewModel? TourVariant;
}