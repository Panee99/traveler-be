using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.Booking;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class BookingService : BaseService, IBookingService
{
    public BookingService(UnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, httpContextAccessor)
    {
    }

    public async Task<Result<BookingViewModel>> Create(Guid travelerId, BookingCreateModel model)
    {
        // validate input
        var passengerQty = model.AdultQuantity + model.ChildrenQuantity + model.InfantQuantity;
        if (passengerQty != model.Passengers.Count)
            return Error.Conflict($"Passenger count does not match: {passengerQty} vs {model.Passengers.Count}");

        // get trip
        var trip = await UnitOfWork.Trips
            .Query()
            .Where(e => e.Id == model.TripId)
            .Include(e => e.Tour)
            .FirstOrDefaultAsync();

        // validate Status
        if (trip is null) return Error.NotFound("Trip not found");

        if (trip.Status != TripStatus.Accepting)
            return Error.Conflict("Can only book Accepting tour trips");

        if (trip.Tour.Status != TourStatus.Active)
            return Error.Conflict("Can only book Active tours");

        // validate input
        if (model.AdultQuantity < 1) return Error.Validation("At least 1 Adult");

        // check if traveler in any other running tour.
        var travelerInOtherTour = await UnitOfWork.Travelers.Query()
            .Where(traveler => traveler.Id == travelerId)
            .SelectMany(traveler => traveler.TourGroups)
            .AnyAsync(tourGroup => tourGroup.Trip.Status != TripStatus.Ended);

        if (travelerInOtherTour) return Error.Conflict("Traveler already in another Tour");

        // check if this tour already booked
        if (await UnitOfWork.Bookings.Query()
                .AnyAsync(e => e.TripId == model.TripId
                               && e.TravelerId == travelerId))
        {
            return Error.Conflict("This tour is already booked");
        }

        var now = DateTimeHelper.VnNow();

        var booking = model.Adapt<Booking>();
        booking.TravelerId = travelerId;
        booking.Status = BookingStatus.Pending;
        booking.Timestamp = now;
        booking.AdultPrice = trip.AdultPrice;
        booking.ChildrenPrice = trip.ChildrenPrice;
        booking.InfantPrice = trip.InfantPrice;
        booking.ExpireAt = now.AddHours(2);

        UnitOfWork.Bookings.Add(booking);

        await UnitOfWork.SaveChangesAsync();

        return booking.Adapt<BookingViewModel>();
    }

    public async Task<Result<List<BookingViewModel>>> ListTravelerBooked(Guid travelerId)
    {
        if (!await UnitOfWork.Travelers.AnyAsync(e => e.Id == travelerId))
            return Error.NotFound("Traveler not exist.");

        var bookings = await UnitOfWork.Bookings
            .Query()
            .Where(e => e.TravelerId == travelerId)
            .Include(e => e.Passengers)
            .ToListAsync();

        return bookings.Adapt<List<BookingViewModel>>();
    }

    public async Task<Result<BookingViewModel>> Cancel(Guid bookingId)
    {
        var booking = await UnitOfWork.Bookings.FindAsync(bookingId);
        if (booking is null) return Error.NotFound("");

        if (booking.Status != BookingStatus.Pending)
            return Error.Conflict("Can only cancel Pending booking");

        booking.Status = BookingStatus.Canceled;

        UnitOfWork.Bookings.Update(booking);
        await UnitOfWork.SaveChangesAsync();

        return booking.Adapt<BookingViewModel>();
    }

    // public async Task<Result<BookingViewModel>> Update(BookingUpdateModel model)
    // {
    //     var booking = await UnitOfWork.Bookings.FindAsync(model.Id);
    //     if (booking is null) return Error.NotFound();
    //
    //     model.AdaptIgnoreNull(booking);
    //
    //     UnitOfWork.Bookings.Update(booking);
    //     await UnitOfWork.SaveChangesAsync();
    //
    //     return booking.Adapt<BookingViewModel>();
    // }
}