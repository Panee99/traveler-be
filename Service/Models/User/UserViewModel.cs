using Data.Enums;

namespace Service.Models.User;

public class UserViewModel
{
    public Guid Id;
    public string Phone = null!;
    public string? Email;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public Gender Gender { get; set; }
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
    public string? AvatarUrl { get; set; }
}