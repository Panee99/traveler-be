using Data.Enums;
using Service.Models.User;

namespace Service.Models.Staff;

public class StaffViewModel : UserViewModel
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime? Birthday { get; set; }
    public Gender Gender { get; set; }
}