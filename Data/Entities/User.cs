using Data.Enums;

namespace Data.Entities;

public class User
{
    public Guid Id { get; set; }

    public string? Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public Gender? Gender { get; set; }

    public UserRole Role { get; set; }

    public UserStatus Status { get; set; }

    public Guid? AvatarId { get; set; }

    public Attachment? Avatar { get; set; }

    public ICollection<FcmToken> FcmTokens = new List<FcmToken>();
}