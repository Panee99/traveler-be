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

    public static string GeneratePassword(int length)
    {
        const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var result = new StringBuilder();
        var random = new Random();

        for (var i = 0; i < length; i++)
        {
            var index = random.Next(validChars.Length);
            result.Append(validChars[index]);
        }

        return result.ToString();
    }
}