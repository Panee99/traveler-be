using Data.Enums;

namespace Service.Models.Traveler;

public class TravelerRegistrationModel
{
    public string IdToken { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public Gender Gender { get; set; }
}