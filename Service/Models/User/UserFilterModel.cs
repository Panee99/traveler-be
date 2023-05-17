using Data.Enums;

namespace Service.Models.User;

public record UserFilterModel : PagingFilterModel
{
    public string? Phone = null!;
    public string? Email;
    public UserRole? Role { get; set; }
    public UserStatus? Status { get; set; }
}