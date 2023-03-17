using Data.Enums;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnassignedField.Global

namespace Service.Models.Traveler;

public record TravelerRegistrationModel
{
    public string IdToken = null!;
    public string Phone = null!;
    public string Password = null!;
    public string FirstName = null!;
    public string LastName = null!;
    public Gender Gender;
}