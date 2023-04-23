using Data.Enums;

namespace Service.Models.TourGuide;

public class TourGuideCreateModel
{
    public string Phone { get; set; } = null!;

    public string? Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public Gender Gender { get; set; }

    public DateTime? Birthday { get; set; }
}