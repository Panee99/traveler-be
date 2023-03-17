using System.Security.Cryptography;

namespace Shared.Auth;

public static class AuthUtils
{
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hashBytes = sha256.ComputeHash(passwordBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        var inputHash = HashPassword(password);
        return inputHash.Equals(hashedPassword);
    }
}