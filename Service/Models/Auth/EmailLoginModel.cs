namespace Service.Models.Auth;

public record EmailLoginModel
(
    string Email,
    string Password
);