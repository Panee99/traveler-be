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

    public BookingService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, httpContextAccessor)
    {
        _bookingRepo = unitOfWork.Repo<Booking>();
        _tourRepo = unitOfWork.Repo<Tour>();
    }

    public async Task<Result<BookingViewModel>> Create(Guid travelerId, BookingCreateModel model)
    {
        var tour = await _tourRepo.FindAsync(model.TourId);
        if (tour is null) return Error.NotFound("Tour not found");

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
}