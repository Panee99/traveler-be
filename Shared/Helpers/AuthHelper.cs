using System.Security.Cryptography;
using System.Text;

namespace Shared.Helpers;

public static class AuthHelper
{
    private static readonly SHA256 Sha256 = SHA256.Create();

    public static string HashPassword(string password)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = Sha256.ComputeHash(passwordBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        var inputHash = HashPassword(password);
        return inputHash.Equals(hashedPassword);
    }
}