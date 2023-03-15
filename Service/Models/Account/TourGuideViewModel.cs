namespace Service.Models.Account;

public class TourGuideViewModel
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public DateTime? Birthday { get; set; }

    public string Gender { get; set; } = null!;
}