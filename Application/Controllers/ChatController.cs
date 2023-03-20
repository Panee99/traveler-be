using Application.Configurations.Auth;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Models.Chat;

namespace Application.Controllers;

[Route("chat")]
public class ChatController : ApiController
{
    [ProducesResponseType(typeof(ChatTokenResponseModel), StatusCodes.Status200OK)]
    [Authorize]
    [HttpGet("token")]
    public async Task<IActionResult> GetToken(Guid groupId)
    {
        // TODO check group access
        var claims = new Dictionary<string, object>
        {
            { "group", groupId }
        };

        var token = await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(CurrentUser.Id.ToString(), claims);

        return Ok(
            new ChatTokenResponseModel(
                CurrentUser.Id,
                groupId,
                token
            ));
    }
}