using Application.Commons;
using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Tag;
using Shared.Enums;

namespace Application.Controllers;

[Route("tags")]
public class TagsController : ApiController
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [ProducesResponseType(typeof(TagViewModel), StatusCodes.Status201Created)]
    [Authorize(UserRole.Manager)]
    [HttpPost("")]
    public IActionResult Create(TagCreateModel model)
    {
        var result = _tagService.Create(model);
        return result.Match(value => CreatedAtAction(nameof(Find), new { value.Id }, value), OnError);
    }

    [ProducesResponseType(typeof(TagViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponsePayload), StatusCodes.Status404NotFound)]
    [Authorize(UserRole.Manager)]
    [HttpPatch("{id:guid}")]
    public IActionResult Update(Guid id, TagUpdateModel model)
    {
        var result = _tagService.Update(id, model);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponsePayload), StatusCodes.Status404NotFound)]
    [Authorize(UserRole.Manager)]
    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var result = _tagService.Delete(id);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(TagViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponsePayload), StatusCodes.Status404NotFound)]
    [HttpGet("{id:guid}")]
    public IActionResult Find(Guid id)
    {
        var result = _tagService.Find(id);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(ICollection<TagViewModel>), StatusCodes.Status200OK)]
    [HttpPost("filter")]
    public async Task<IActionResult> Filter(TagFilterModel model)
    {
        var result = await _tagService.Filter(model);
        return result.Match(Ok, OnError);
    }
}