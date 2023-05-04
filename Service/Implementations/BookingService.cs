using Data.EFCore;
using Data.EFCore.Repositories;
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
    private readonly IRepository<Booking> _bookingRepo;
    private readonly IRepository<Tour> _tourRepo;
    private readonly IRepository<Traveler> _travelerRepo;

    public BookingService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, httpContextAccessor)
    {
        _bookingRepo = unitOfWork.Repo<Booking>();
        _tourRepo = unitOfWork.Repo<Tour>();
        _travelerRepo = unitOfWork.Repo<Traveler>();
    }

    public async Task<Result<BookingViewModel>> Create(Guid travelerId, BookingCreateModel model)
    {
        var tour = await _tourRepo.FindAsync(model.TourId);
        if (tour is null) return Error.NotFound("Tour not found");

        // check if traveler in any active tour.
        var travelerInActiveTour = await _travelerRepo.Query()
            .Where(traveler => traveler.Id == travelerId)
            .SelectMany(traveler => traveler.TravelerInTours)
            .Select(travelerInTour => travelerInTour.Tour)
            .Where(t => t.Status == TourStatus.Active)
            .FirstOrDefaultAsync();

        if (travelerInActiveTour != null)
            return Error.Conflict("Traveler already in an active tour");

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

        _bookingRepo.Add(booking);

        await UnitOfWork.SaveChangesAsync();

        return booking.Adapt<BookingViewModel>();
    }

    public async Task<Result<BookingViewModel>> Update(BookingUpdateModel model)
    {
        var booking = await _bookingRepo.FindAsync(model.Id);
        if (booking is null) return Error.NotFound();

        model.AdaptIgnoreNull(booking);

        _bookingRepo.Update(booking);
        await UnitOfWork.SaveChangesAsync();

        return booking.Adapt<BookingViewModel>();
    }

    public async Task<Result<List<BookingViewModel>>> Filter(BookingFilterModel model)
    {
        if (AuthUser!.Role == AccountRole.Traveler)
            if (AuthUser.Id != model.TravelerId)
                return Error.Authorization();

        var bookings = await _bookingRepo.Query().Where(e => e.TravelerId == model.TravelerId).ToListAsync();

        return bookings.Adapt<List<BookingViewModel>>();
    }

    public async Task<Result<List<BookingViewModel>>> ListTravelerBooked(Guid travelerId)
    {
        if (!await _travelerRepo.AnyAsync(e => e.Id == travelerId))
            return Error.NotFound("Traveler not exist.");

        var bookings = await _bookingRepo
            .Query()
            .Where(e => e.TravelerId == travelerId)
            .ToListAsync();

        return bookings.Adapt<List<BookingViewModel>>();
    }
}