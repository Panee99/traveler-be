using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Tour;
using Shared.Enums;
using Shared.Helpers;

namespace Application.Controllers;

[Route("tours")]
public class ToursController : ApiController
{
    private readonly ITourService _tourService;

    public ToursController(ITourService tourService)
    {
        _tourService = tourService;
    }

    [Authorize(UserRole.Manager)]
    [HttpPost("")]
    public async Task<IActionResult> Create(TourCreateModel model)
    {
        var result = await _tourService.Create(model);
        return result.Match(Ok, OnError);
    }

    [Authorize(UserRole.Manager)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, TourUpdateModel model)
    {
        var result = await _tourService.Update(id, model);
        return result.Match(Ok, OnError);
    }

    [Authorize(UserRole.Manager)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _tourService.Delete(id);
        return result.Match(Ok, OnError);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Find(Guid id)
    {
        var result = await _tourService.Find(id);
        return result.Match(Ok, OnError);
    }

    [Authorize(UserRole.Manager)]
    [HttpPut("{id:guid}/thumbnail")]
    public async Task<IActionResult> UpdateThumbnail(Guid id, IFormFile file)
    {
        var validateResult = FileHelper.ValidateImageFile(file);
        if (!validateResult.IsSuccess) return OnError(validateResult.Error);

        var result = await _tourService.UpdateThumbnail(id, file.ContentType, file.OpenReadStream());
        return result.Match(Ok, OnError);
    }

    // public async Task<IActionResult> Filter(TourFilterModel model)
    // {
    // }
}


