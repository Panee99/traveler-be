using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Models.Chat;

namespace Application.Controllers;

[Route("chat")]
public class ChatController : ApiController
{
    [HttpGet("token")]
    public async Task<IActionResult> GetToken()
    {
        const string userId = "8831C0B0-DDBA-4758-B995-2F698391ABB5";
        var token = await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(userId);
        return Ok(new TokenResponseModel()
        {
            UserId = Guid.Parse(userId),
            Token = token
        });
    }
}