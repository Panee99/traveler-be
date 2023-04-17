using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Booking;

namespace Application.Controllers;

[Route("bookings")]
public class BookingsController : ApiController
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [Authorize(AccountRole.Traveler)]
    [HttpPost("")]
    public async Task<IActionResult> Create(BookingCreateModel model)
    {
        var result = await _bookingService.Create(CurrentUser.Id, model);
        return result.Match(Ok, OnError);
    }
}