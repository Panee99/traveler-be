using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Booking;
using Shared.ResultExtensions;

namespace Application.Controllers;

[Authorize(UserRole.Traveler)]
[Route("bookings")]
public class BookingsController : ApiController
{
    private readonly IBookingService _bookingService;
    private readonly ITransactionService _transactionService;

    public BookingsController(IBookingService bookingService, ITransactionService transactionService)
    {
        _bookingService = bookingService;
        _transactionService = transactionService;
    }

    /// <summary>
    /// Create new tour booking
    /// </summary>
    [HttpPost("")]
    public async Task<IActionResult> Create(BookingCreateModel model)
    {
        var result = await _bookingService.Create(CurrentUser.Id, model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Cancel a booking
    /// </summary>
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _bookingService.Cancel(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Create pay url for a booking
    /// </summary>
    [HttpGet("{id}/pay")]
    public async Task<IActionResult> CreatePayTransaction(Guid id)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        if (clientIp is null) return OnError(Error.Unexpected("Client IP unknown"));

        var result = await _transactionService.CreateTransaction(id, clientIp);
        return result.Match(Ok, OnError);
    }
}