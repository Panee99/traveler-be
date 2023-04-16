﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Data.EFCore;
using Data.EFCore.Repositories;
using Data.Entities;
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

    //
    private readonly IRepository<Account> _accountRepo;

    public AuthService(IUnitOfWork unitOfWork, IOptions<AppSettings> appSettings) : base(unitOfWork)
    {
        _appSettings = appSettings.Value;
        _accountRepo = unitOfWork.Repo<Account>();
    }

    public async Task<Result<AuthenticateResponseModel>> Authenticate(LoginModel model)
    {
        var query = _accountRepo.Query();

        // By Phone
        if (PhoneRegex.Match(model.Username).Success) query = query.Where(e => e.Phone == model.Username);
        // By Email
        else if (EmailRegex.Match(model.Username).Success) query = query.Where(e => e.Email == model.Username);
        // Error
        else return Error.Validation("Login by Phone or Email");

        var account = await query.Select(e => new AuthResult(e.Id, e.Password, e.Role)).FirstOrDefaultAsync();

        if (account == null || !AuthHelper.VerifyPassword(model.Password, account.Password))
            return Error.Authentication();

        return new AuthenticateResponseModel(_generateJwtToken(account.Id, account.Role));
    }

    private string _generateJwtToken(Guid accountId, AccountRole role)
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

    private record AuthResult
    (
        Guid Id,
        string Password,
        AccountRole Role
    );
}