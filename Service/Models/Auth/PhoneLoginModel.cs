// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace Service.Models.Auth;

public record PhoneLoginModel
{
    public string Password = null!;
    public string Phone = null!;
}