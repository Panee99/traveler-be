using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Service.Interfaces;
using Service.Models.Auth;
using Shared.Enums;
using Shared.Helpers;
using Shared.ResultExtensions;
using Shared.Settings;

namespace Service.Implementations;

public class AuthService : BaseService, IAuthService
{
    private readonly AppSettings _appSettings;

    public AuthService(IUnitOfWork unitOfWork, IOptions<AppSettings> appSettings) : base(unitOfWork)
    {
        _appSettings = appSettings.Value;
    }

    public async Task<Result<AuthenticateResponseModel>> AuthenticateTraveler(PhoneLoginModel model)
    {
        var traveler = await _unitOfWork.Repo<Traveler>()
            .Query()
            .FirstOrDefaultAsync(e => e.Phone == model.Phone && e.Status == AccountStatus.ACTIVE);

        if (traveler == null || !AuthHelper.VerifyPassword(model.Password, traveler.Password))
            return Error.Authentication();

        return new AuthenticateResponseModel(_generateJwtToken(traveler.Id, UserRole.Traveler));
    }

    public async Task<Result<AuthenticateResponseModel>> AuthenticateManager(EmailLoginModel model)
    {
        var manager = await _unitOfWork.Repo<Manager>()
            .Query()
            .FirstOrDefaultAsync(e => e.Email == model.Email && e.Status == AccountStatus.ACTIVE);

        if (manager == null || !AuthHelper.VerifyPassword(model.Password, manager.Password))
            return Error.Authentication();

        return new AuthenticateResponseModel(_generateJwtToken(manager.Id, UserRole.Manager));
    }

    public async Task<Result<AuthenticateResponseModel>> AuthenticateTourGuide(EmailLoginModel model)
    {
        var tourGuide = await _unitOfWork.Repo<TourGuide>()
            .Query()
            .FirstOrDefaultAsync(e => e.Email == model.Email && e.Status == AccountStatus.ACTIVE);

        if (tourGuide == null || !AuthHelper.VerifyPassword(model.Password, tourGuide.Password))
            return Error.Authentication();

        return new AuthenticateResponseModel(_generateJwtToken(tourGuide.Id, UserRole.TourGuide));
    }


    // PRIVATE
    private static readonly JwtSecurityTokenHandler TokenHandler = new();
    private string _generateJwtToken(Guid accountId, UserRole role)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", accountId.ToString()),
                    new Claim("role", role.ToString())
                }
            ),
            Issuer = _appSettings.JwtIssuer,
            Audience = _appSettings.JwtAudience,
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials =
                new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.JwtSecret)),
                    SecurityAlgorithms.HmacSha256Signature)
        };


        return TokenHandler.WriteToken(TokenHandler.CreateToken(tokenDescriptor));
    }
}