using System.Security.Cryptography;
using System.Text;

namespace Shared.Auth;

public static class AuthHelper
{
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = sha256.ComputeHash(passwordBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        var inputHash = HashPassword(password);
        return inputHash.Equals(hashedPassword);
    }
}