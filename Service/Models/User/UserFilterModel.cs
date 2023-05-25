using Data.Enums;

namespace Service.Models.User;

public record UserFilterModel : PagingFilterModel
{
    public string? Phone;
    public string? Email;
    public string? FirstName;
    public string? LastName;
    public Gender? Gender;
    public UserRole? Role;
    public UserStatus? Status;
}