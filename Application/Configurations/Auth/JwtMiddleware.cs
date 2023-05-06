using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Data.EFCore;
using Data.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Service.Models.Auth;
using Shared;
using Shared.Settings;

namespace Application.Configurations.Auth;

public class JwtMiddleware : IMiddleware
{
    private readonly AppSettings _appSettings;
    private readonly ILogger<JwtMiddleware> _logger;
    private readonly UnitOfWork _unitOfWork;

    public JwtMiddleware(
        IOptions<AppSettings> appSettings,
        UnitOfWork unitOfWork,
        ILogger<JwtMiddleware> logger)
    {
        _appSettings = appSettings.Value;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var values))
        {
            var token = values.FirstOrDefault();
            if (token != null) await _verifyToken(context, token);
        }

        await next(context);
    }

    private async Task _verifyToken(HttpContext context, string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _appSettings.JwtIssuer,
                ValidAudience = _appSettings.JwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.JwtSecret))
            }, out var securityToken);

            var jwtToken = (JwtSecurityToken)securityToken;

            var id = Guid.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
            var role = Enum.Parse<AccountRole>(jwtToken.Claims.First(x => x.Type == "role").Value);

            if (!await _checkUserClaims(id, role)) return;
            context.Items[AppConstants.UserContextKey] = new AuthUser(id, role);
        }

        catch (Exception e)
        {
            _logger.LogDebug("{Message}", e.Message);
        }
    }

    private async Task<bool> _checkUserClaims(Guid id, AccountRole role)
    {
        return role switch
        {
            AccountRole.Manager => await _unitOfWork.Managers.AnyAsync(
                e => e.Id == id && e.Status == AccountStatus.Active),
            AccountRole.Traveler => await _unitOfWork.Travelers.AnyAsync(
                e => e.Id == id && e.Status == AccountStatus.Active),
            AccountRole.TourGuide => await _unitOfWork.TourGuides.AnyAsync(
                e => e.Id == id && e.Status == AccountStatus.Active),
            _ => throw new ArgumentOutOfRangeException(typeof(JwtMiddleware).ToString())
        };
    }
}