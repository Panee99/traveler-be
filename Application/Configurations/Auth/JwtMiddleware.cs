using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Data;
using Data.Entities;
using Data.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Enums;
using Shared.Settings;

namespace Application.Configurations.Auth;

public class JwtMiddleware : IMiddleware
{
    private readonly AppSettings _appSettings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<JwtMiddleware> _logger;

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
            if (token != null) _verifyToken(context, token);
        }

        await next(context);
    }

    private void _verifyToken(HttpContext context, string token)
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
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.JwtSecret)),
            }, out var securityToken);

            var jwtToken = (JwtSecurityToken)securityToken;

            var id = Guid.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
            var role = Enum.Parse<UserRole>(jwtToken.Claims.First(x => x.Type == "role").Value);

            if (_checkUserClaims(id, role)) return;

            context.Items["User"] = new AuthUser(id, role);
        }

        catch (Exception e)
        {
            _logger.LogDebug("{Message}", e.Message);
        }
    }

    private bool _checkUserClaims(Guid id, UserRole role)
    {
        return role switch
        {
            UserRole.Manager => _unitOfWork.Repo<TourGuide>().Any(e => e.Id == id && e.Status == AccountStatus.ACTIVE),
            UserRole.Traveler => _unitOfWork.Repo<Traveler>().Any(e => e.Id == id && e.Status == AccountStatus.ACTIVE),
            UserRole.TourGuide => _unitOfWork.Repo<TourGuide>().Any(e => e.Id == id && e.Status == AccountStatus.ACTIVE),
            _ => throw new ArgumentOutOfRangeException(typeof(JwtMiddleware).ToString())
        };
    }
}