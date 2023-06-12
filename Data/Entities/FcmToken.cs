namespace Data.Entities;

public class FcmToken
{
    public Guid Id { get; set; }

    public string Token { get; set; } = null!;

    public User User { get; set; } = null!;

    //
    public Guid UserId { get; set; }
}