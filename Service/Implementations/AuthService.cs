using Data;
using Data.Models.Get;
using Data.Models.View;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Service.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Utility.Enums;
using Utility.Settings;

namespace Service.Implementations;

public class AuthService : BaseService, IAuthService
{
    private readonly IManagerRepository _managerRepository;
    private readonly ITravelerRepository _travelerRepository;
    private readonly ITourGuideRepository _tourGuideRepository;
    private readonly AppSetting _appSettings;

    public AuthService(IUnitOfWork unitOfWork, IOptions<AppSetting> appSettings) : base(unitOfWork)
    {
        _managerRepository = unitOfWork.Manager;
        _travelerRepository = unitOfWork.Traveler;
        _tourGuideRepository = unitOfWork.TourGuide;
        _appSettings = appSettings.Value;
    }

    public async Task<TokenViewModel> AuthenticateManager(AuthRequestModel model)
    {
        var manager = await _managerRepository.GetMany(manager =>
                manager.Account.Email.Equals(model.Email) && manager.Account.Password.Equals(model.Password))
            .Include(manager => manager.Account)
            .FirstOrDefaultAsync();
        if (manager != null)
        {
            var token = GenerateJwtToken(new AuthViewModel
            {
                Id = manager.Id,
                Role = UserRole.Manager.ToString(),
                Status = manager.Account.Status,
            });
            return new TokenViewModel
            {
                Token = token
            };
        }

        return null!;
    }

    public async Task<TokenViewModel> AuthenticateTraveler(AuthRequestModel model)
    {
        var traveler = await _travelerRepository.GetMany(traveler =>
                traveler.Account.Email.Equals(model.Email) && traveler.Account.Password.Equals(model.Password))
            .Include(traveler => traveler.Account)
            .FirstOrDefaultAsync();
        if (traveler != null)
        {
            var token = GenerateJwtToken(new AuthViewModel
            {
                Id = traveler.Id,
                Role = UserRole.Traveler.ToString(),
                Status = traveler.Account.Status,
            });
            return new TokenViewModel
            {
                Token = token
            };
        }

        return null!;
    }

    public async Task<TokenViewModel> AuthenticateTourGuide(AuthRequestModel model)
    {
        var tourGuide = await _tourGuideRepository.GetMany(tourGuide =>
                tourGuide.Account.Email.Equals(model.Email) && tourGuide.Account.Password.Equals(model.Password))
            .Include(tourGuide => tourGuide.Account)
            .FirstOrDefaultAsync();
        if (tourGuide != null)
        {
            var token = GenerateJwtToken(new AuthViewModel
            {
                Id = tourGuide.Id,
                Role = UserRole.TourGuide.ToString(),
                Status = tourGuide.Account.Status,
            });
            return new TokenViewModel
            {
                Token = token
            };
        }

        return null!;
    }

    public async Task<ManagerViewModel> GetManagerById(Guid id)
    {
        var manager = await _managerRepository.GetMany(manager => manager.Id.Equals(id))
            .Include(manager => manager.Account)
            .FirstOrDefaultAsync();
        if (manager != null)
        {
            return new ManagerViewModel
            {
                Id = manager.Id,
                AvatarUrl = manager.AvatarUrl,
                Birthday = manager.Birthday,
                FirstName = manager.FirstName,
                LastName = manager.LastName,
                Gender = manager.Gender,
            };
        }

        return null!;
    }

    public async Task<TourGuideViewModel> GetTourGuideById(Guid id)
    {
        var tourGuide = await _tourGuideRepository.GetMany(tourGuide => tourGuide.Id.Equals(id))
            .Include(tourGuide => tourGuide.Account)
            .FirstOrDefaultAsync();
        if (tourGuide != null)
        {
            return new TourGuideViewModel
            {
                Id = tourGuide.Id,
                AvatarUrl = tourGuide.AvatarUrl,
                Birthday = tourGuide.Birthday,
                FirstName = tourGuide.FirstName,
                LastName = tourGuide.LastName,
                Gender = tourGuide.Gender,
            };
        }

        return null!;
    }

    public async Task<TravelerViewModel> GetTravelerById(Guid id)
    {
        var traveler = await _travelerRepository.GetMany(traveler => traveler.Id.Equals(id))
            .Include(traveler => traveler.Account)
            .FirstOrDefaultAsync();
        if (traveler != null)
        {
            return new TravelerViewModel
            {
                Id = traveler.Id,
                AvatarUrl = traveler.AvatarUrl,
                Birthday = traveler.Birthday,
                FirstName = traveler.FirstName,
                LastName = traveler.LastName,
                Gender = traveler.Gender,
                Address = traveler.Address,
            };
        }

        return null!;
    }

    public async Task<AuthViewModel> AuthById(Guid id)
    {
        if (_managerRepository.Any(manager => manager.Id.Equals(id)))
        {
            return await _managerRepository.GetMany(manager => manager.Id.Equals(id)).Select(manager =>
                new AuthViewModel
                {
                    Id = manager.Id,
                    Role = UserRole.Manager.ToString(),
                    Status = manager.Account.Status
                }).FirstOrDefaultAsync() ?? null!;
        }

        if (_tourGuideRepository.Any(tourGuide => tourGuide.Id.Equals(id)))
        {
            return await _tourGuideRepository.GetMany(tourGuide => tourGuide.Id.Equals(id)).Select(tourGuide =>
                new AuthViewModel
                {
                    Id = tourGuide.Id,
                    Role = UserRole.TourGuide.ToString(),
                    Status = tourGuide.Account.Status
                }).FirstOrDefaultAsync() ?? null!;
        }

        if (_travelerRepository.Any(traveler => traveler.Id.Equals(id)))
        {
            return await _travelerRepository.GetMany(traveler => traveler.Id.Equals(id)).Select(traveler =>
                new AuthViewModel
                {
                    Id = traveler.Id,
                    Role = UserRole.Traveler.ToString(),
                    Status = traveler.Account.Status
                }).FirstOrDefaultAsync() ?? null!;
        }

        return null!;
    }

    private string GenerateJwtToken(AuthViewModel model)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", model.Id.ToString()),
                new Claim("role", model.Role),
                new Claim("status", model.Status),
            }),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}