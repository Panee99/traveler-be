namespace Data.Models.View;

public class TravelerViewModel
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime? Birthday { get; set; }

    public string? AvatarUrl { get; set; }

    public string Gender { get; set; } = null!;

    public string? Address { get; set; }
}