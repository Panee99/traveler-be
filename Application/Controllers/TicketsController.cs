using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Ticket;
using Shared.Helpers;

namespace Application.Controllers;

[Route("tickets")]
public class TicketsController : ApiController
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [Authorize(AccountRole.Manager, AccountRole.TourGuide)]
    [HttpPost("")]
    public async Task<IActionResult> Create(TicketCreateModel model)
    {
        var result = await _ticketService.Create(model);
        return result.Match(Ok, OnError);
    }

    [Authorize(AccountRole.Manager, AccountRole.TourGuide)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _ticketService.Delete(id);
        return result.Match(Ok, OnError);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Find(Guid id)
    {
        var result = await _ticketService.Find(id);
        return result.Match(Ok, OnError);
    }

    [Authorize]
    [HttpPost("filter")]
    public async Task<IActionResult> Filter(TicketFilterModel model)
    {
        var result = await _ticketService.Filter(model);
        return result.Match(Ok, OnError);
    }

    [Authorize(AccountRole.Manager, AccountRole.TourGuide)]
    [HttpPut("{id:guid}/image")]
    public async Task<IActionResult> UpdateImage([FromRoute] Guid id, IFormFile file)
    {
        var validateResult = FileHelper.ValidateImageFile(file);
        if (!validateResult.IsSuccess) return OnError(validateResult.Error);

        var result = await _ticketService.UpdateImage(id, file.ContentType, file.OpenReadStream());
        return result.Match(Ok, OnError);
    }
}