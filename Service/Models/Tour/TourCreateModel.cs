﻿using Data.Enums;
using Service.Models.TourFlow;

namespace Service.Models.Tour;

public record TourCreateModel
(
    string Title,
    double AdultPrice,
    double ChildrenPrice,
    double InfantPrice,
    string Departure,
    string Destination,
    DateTime StartTime,
    DateTime EndTime,
    int MaxOccupancy,
    TourType Type,
    string? Description,
    List<TourFlowCreateModel> TourFlows,
    Guid? ThumbnailId
);