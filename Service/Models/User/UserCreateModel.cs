using Data.Enums;

namespace Service.Models.User;

public record UserCreateModel
{
    public string? Phone = null!;
    public string Email = null!;
    public string Password = null!;
    public UserRole Role;
    public UserStatus Status;
    public string FirstName = null!;
    public string LastName = null!;
    public Gender? Gender;
}