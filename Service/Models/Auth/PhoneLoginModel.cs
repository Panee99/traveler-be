// ReSharper disable FieldCanBeMadeReadOnly.Global
namespace Service.Models.Auth;

public record PhoneLoginModel
{
    public string Phone = null!;
    public string Password = null!;
}