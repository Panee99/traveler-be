using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Ticket;

namespace Application.Controllers;

[Route("tickets")]
public class TicketsController : ApiController
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [Authorize(UserRole.Admin, UserRole.TourGuide)]
    [ProducesResponseType(typeof(TicketViewModel), StatusCodes.Status200OK)]
    [HttpPost("")]
    public async Task<IActionResult> Create(TicketCreateModel model)
    {
        var result = await _ticketService.Create(model);
        return result.Match(Ok, OnError);
    }

    [Authorize(UserRole.Admin, UserRole.TourGuide)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _ticketService.Delete(id);
        return result.Match(Ok, OnError);
    }

    [Authorize]
    [ProducesResponseType(typeof(TicketViewModel), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Find(Guid id)
    {
        var result = await _ticketService.Find(id);
        return result.Match(Ok, OnError);
    }

    [Authorize]
    [ProducesResponseType(typeof(List<TicketViewModel>), StatusCodes.Status200OK)]
    [HttpPost("filter")]
    public async Task<IActionResult> Filter(TicketFilterModel model)
    {
        var result = await _ticketService.Filter(model);
        return result.Match(Ok, OnError);
    }
}