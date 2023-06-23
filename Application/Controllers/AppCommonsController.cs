using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[Route("app")]
public class AppCommonsController : ApiController
{
    [HttpGet("contacts")]
    public IActionResult GetEmergencyContacts()
    {
        return Ok(new
        {
            FirstContact = "0378992633",
            SecondContact = "0389376290"
        });
    }
}