using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared;
using Shared.Enums;
using Shared.Settings;

namespace Application.Configurations.Auth;

public class JwtMiddleware : IMiddleware
{
    private readonly AppSettings _appSettings;
    private readonly ILogger<JwtMiddleware> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public JwtMiddleware(
        IOptions<AppSettings> appSettings,
        IUnitOfWork unitOfWork,
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
            var role = Enum.Parse<UserRole>(jwtToken.Claims.First(x => x.Type == "role").Value);

            if (!await _checkUserClaims(id, role)) return;
            context.Items[AppConstants.UserContextKey] = new AuthUser(id, role);
        }

        catch (Exception e)
        {
            _logger.LogDebug("{Message}", e.Message);
        }
    }

    private async Task<bool> _checkUserClaims(Guid id, UserRole role)
    {
        return role switch
        {
            UserRole.Manager => await _unitOfWork.Repo<Manager>()
                .AnyAsync(e => e.Id == id && e.Status == AccountStatus.ACTIVE),
            UserRole.Traveler => await _unitOfWork.Repo<Traveler>()
                .AnyAsync(e => e.Id == id && e.Status == AccountStatus.ACTIVE),
            UserRole.TourGuide => await _unitOfWork.Repo<TourGuide>()
                .AnyAsync(e => e.Id == id && e.Status == AccountStatus.ACTIVE),
            _ => throw new ArgumentOutOfRangeException(typeof(JwtMiddleware).ToString())
        };
    }
}