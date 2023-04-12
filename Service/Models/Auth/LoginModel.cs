// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace Service.Models.Auth;

public record LoginModel
(
    string Username,
    string Password
);