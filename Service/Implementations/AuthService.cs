using Data;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Service.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Data.Entities;
using Data.Enums;
using Service.Models.Auth;
using Shared.Auth;
using Shared.Enums;
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

    public Result<AuthenticateResponseModel> AuthenticateTraveler(PhoneLoginModel model)
    {
        var traveler = _unitOfWork.Repo<Traveler>()
            .Query()
            .FirstOrDefault(e => e.Phone == model.Phone && e.Status == AccountStatus.ACTIVE);

        if (traveler == null || !AuthHelper.VerifyPassword(model.Password, traveler.Password))
            return Error.Authentication();

        return new AuthenticateResponseModel(Token: _generateJwtToken(traveler.Id, UserRole.Traveler));
    }

    public Result<AuthenticateResponseModel> AuthenticateManager(EmailLoginModel model)
    {
        var manager = _unitOfWork.Repo<Manager>()
            .Query()
            .FirstOrDefault(e => e.Email == model.Email && e.Status == AccountStatus.ACTIVE);

        if (manager == null || !AuthHelper.VerifyPassword(model.Password, manager.Password))
            return Error.Authentication();

        return new AuthenticateResponseModel(Token: _generateJwtToken(manager.Id, UserRole.Manager));
    }

    public Result<AuthenticateResponseModel> AuthenticateTourGuide(EmailLoginModel model)
    {
        var tourGuide = _unitOfWork.Repo<TourGuide>()
            .Query()
            .FirstOrDefault(e => e.Email == model.Email && e.Status == AccountStatus.ACTIVE);

        if (tourGuide == null || !AuthHelper.VerifyPassword(model.Password, tourGuide.Password))
            return Error.Authentication();

        return new AuthenticateResponseModel(Token: _generateJwtToken(tourGuide.Id, UserRole.TourGuide));
    }


    // PRIVATE
    private string _generateJwtToken(Guid accountId, UserRole role)
    {
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", accountId.ToString()),
                    new Claim("role", role.ToString()),
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

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}