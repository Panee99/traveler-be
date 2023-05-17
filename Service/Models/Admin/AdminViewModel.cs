using Data.Enums;
using Service.Models.User;

namespace Service.Models.Admin;

public class AdminViewModel : UserViewModel
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime? Birthday { get; set; }
    public Gender Gender { get; set; }
}