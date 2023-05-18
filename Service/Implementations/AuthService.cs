using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Data.EFCore;
using Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Service.Interfaces;
using Service.Models.Auth;
using Shared.Helpers;
using Shared.ResultExtensions;
using Shared.Settings;

namespace Service.Implementations;

public class AuthService : BaseService, IAuthService
{
    private static readonly Regex PhoneRegex = new(@"^\+?\d{7,15}$");
    private static readonly Regex EmailRegex = new(@"^\S+@\S+\.\S+$");
    private static readonly JwtSecurityTokenHandler TokenHandler = new();
    private readonly AppSettings _appSettings;

    private const int ExpirationDay = int.MaxValue;
    
    public AuthService(UnitOfWork unitOfWork, IOptions<AppSettings> appSettings) : base(unitOfWork)
    {
        _appSettings = appSettings.Value;
    }

    public async Task<Result<AuthenticateResponseModel>> Authenticate(LoginModel model)
    {
        var query = UnitOfWork.Users.Query();

        // By Phone
        if (PhoneRegex.Match(model.Username).Success) query = query.Where(e => e.Phone == model.Username);
        // By Email
        else if (EmailRegex.Match(model.Username).Success) query = query.Where(e => e.Email == model.Username);
        // Error
        else return Error.Validation("Login by Phone or Email");

        var user = await query.Select(e => new AuthResult(e.Id, e.Password, e.Role)).FirstOrDefaultAsync();

        if (user == null || !AuthHelper.VerifyPassword(model.Password, user.Password))
            return Error.Authentication();

        return new AuthenticateResponseModel(_generateJwtToken(user.Id, user.Role));
    }

    private string _generateJwtToken(Guid userId, UserRole role)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", userId.ToString()),
                    new Claim("role", role.ToString())
                }
            ),
            Issuer = _appSettings.JwtIssuer,
            Audience = _appSettings.JwtAudience,
            Expires = DateTime.Now.AddDays(ExpirationDay),
            SigningCredentials =
                new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.JwtSecret)),
                    SecurityAlgorithms.HmacSha256Signature)
        };

        return TokenHandler.WriteToken(TokenHandler.CreateToken(tokenDescriptor));
    }

    private record AuthResult
    (
        Guid Id,
        string Password,
        UserRole Role
    );
}