using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Commons;
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
        var tour = await UnitOfWork.Tours.FindAsync(model.TourId);
        if (tour is null) return Error.NotFound("Tour not found");

        if (tour.Status != TourStatus.Active)
            return Error.Conflict("Can only book status Active tours");

        // check if traveler in any active tour.
        var travelerInActiveTour = await UnitOfWork.Travelers.Query()
            .Where(traveler => traveler.Id == travelerId)
            .SelectMany(traveler => traveler.TravelerInTours)
            .Select(travelerInTour => travelerInTour.Tour)
            .Where(t => t.Status != TourStatus.Closed)
            .FirstOrDefaultAsync();

        if (travelerInActiveTour != null)
            return Error.Conflict("Traveler already in an tour");

        var booking = new Booking()
        {
            TourId = model.TourId,
            TravelerId = travelerId,
            AdultQuantity = model.AdultQuantity,
            ChildrenQuantity = model.ChildrenQuantity,
            InfantQuantity = model.InfantQuantity,
            PaymentStatus = PaymentStatus.Pending,
            Timestamp = DateTimeHelper.VnNow(),
            AdultPrice = tour.AdultPrice,
            ChildrenPrice = tour.ChildrenPrice,
            InfantPrice = tour.InfantPrice,
        };

        UnitOfWork.Bookings.Add(booking);

        await UnitOfWork.SaveChangesAsync();

        return booking.Adapt<BookingViewModel>();
    }

    public async Task<Result<BookingViewModel>> Update(BookingUpdateModel model)
    {
        var booking = await UnitOfWork.Bookings.FindAsync(model.Id);
        if (booking is null) return Error.NotFound();

        model.AdaptIgnoreNull(booking);

        UnitOfWork.Bookings.Update(booking);
        await UnitOfWork.SaveChangesAsync();

        return booking.Adapt<BookingViewModel>();
    }

    public async Task<Result<List<BookingViewModel>>> Filter(BookingFilterModel model)
    {
        if (CurrentUser!.Role == AccountRole.Traveler)
            if (CurrentUser.Id != model.TravelerId)
                return Error.Authorization();

        var bookings = await UnitOfWork.Bookings.Query().Where(e => e.TravelerId == model.TravelerId).ToListAsync();

        return bookings.Adapt<List<BookingViewModel>>();
    }

    public async Task<Result<List<BookingViewModel>>> ListTravelerBooked(Guid travelerId)
    {
        if (!await UnitOfWork.Travelers.AnyAsync(e => e.Id == travelerId))
            return Error.NotFound("Traveler not exist.");

        var bookings = await UnitOfWork.Bookings
            .Query()
            .Where(e => e.TravelerId == travelerId)
            .ToListAsync();

        return bookings.Adapt<List<BookingViewModel>>();
    }
}