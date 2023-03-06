namespace Data.Models.View
{
    public class AuthViewModel
    {
        public Guid Id { get; set; }
        public string Role { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
