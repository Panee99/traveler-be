using Data.Enums;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnassignedField.Global

namespace Service.Models.Traveler;

public record TravelerRegistrationModel
{
    public string FirstName = null!;
    public Gender Gender;
    public string? IdToken = null!;
    public string LastName = null!;
    public string Password = null!;
    public string Phone = null!;
}