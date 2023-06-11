﻿using Service.Models.Booking;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IBookingService
{
    Task<Result<BookingViewModel>> Create(Guid travelerId, BookingCreateModel model);

    Task<Result<List<BookingViewModel>>> ListTravelerBooked(Guid travelerId);

    Task<Result<BookingViewModel>> Cancel(Guid bookingId);
}