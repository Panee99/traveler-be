using Data.Enums;
using Service.Models.Account;

namespace Service.Models.Manager;

public class ManagerViewModel : AccountViewModel
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime? Birthday { get; set; }
    public Gender Gender { get; set; }
}