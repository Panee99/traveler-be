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
using Ultility.Enums;
using Utility.Settings;

namespace Service.Implementations
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly IManagerRepository _managerRepository;
        private readonly ITravellerRepository _travellerRepository;
        private readonly ITourGuideRepository _tourGuideRepository;
        private readonly AppSetting _appSettings;

        public AuthService(IUnitOfWork unitOfWork, IOptions<AppSetting> appSettings) : base(unitOfWork)
        {
            _managerRepository = unitOfWork.Manager;
            _travellerRepository = unitOfWork.Traveller;
            _tourGuideRepository = unitOfWork.TourGuide;
            _appSettings = appSettings.Value;
        }

        public async Task<TokenViewModel> AuthenticateManager(AuthRequestModel model)
        {
            var manager = await _managerRepository.GetMany(manager => manager.Account.Email.Equals(model.Email) && manager.Account.Password.Equals(model.Password))
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

        public async Task<TokenViewModel> AuthenticateTraveller(AuthRequestModel model)
        {
            var traveller = await _travellerRepository.GetMany(traveller => traveller.Account.Email.Equals(model.Email) && traveller.Account.Password.Equals(model.Password))
                .Include(traveller => traveller.Account)
                .FirstOrDefaultAsync();
            if (traveller != null)
            {
                var token = GenerateJwtToken(new AuthViewModel
                {
                    Id = traveller.Id,
                    Role = UserRole.Traveller.ToString(),
                    Status = traveller.Account.Status,
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
            var tourGuide = await _tourGuideRepository.GetMany(tourGuide => tourGuide.Account.Email.Equals(model.Email) && tourGuide.Account.Password.Equals(model.Password))
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
                   Id= manager.Id,
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

        public async Task<TravellerViewModel> GetTravellerById(Guid id)
        {
            var traveller = await _travellerRepository.GetMany(traveller => traveller.Id.Equals(id))
                .Include(traveller => traveller.Account)
                .FirstOrDefaultAsync();
            if (traveller != null)
            {
                return new TravellerViewModel
                {
                    Id = traveller.Id,
                    AvatarUrl = traveller.AvatarUrl,
                    Birthday = traveller.Birthday,
                    FirstName = traveller.FirstName,
                    LastName = traveller.LastName,
                    Gender = traveller.Gender,
                    Address = traveller.Address,
                };
            }
            return null!;
        }
        public async Task<AuthViewModel> AuthById(Guid id)
        {
            if (_managerRepository.Any(manager => manager.Id.Equals(id)))
            {
                return await _managerRepository.GetMany(manager => manager.Id.Equals(id)).Select(manager => new AuthViewModel
                {
                    Id = manager.Id,
                    Role = UserRole.Manager.ToString(),
                    Status = manager.Account.Status
                }).FirstOrDefaultAsync() ?? null!;
            }
            if (_tourGuideRepository.Any(tourGuide => tourGuide.Id.Equals(id)))
            {
                return await _tourGuideRepository.GetMany(tourGuide => tourGuide.Id.Equals(id)).Select(tourGuide => new AuthViewModel
                {
                    Id = tourGuide.Id,
                    Role = UserRole.TourGuide.ToString(),
                    Status = tourGuide.Account.Status
                }).FirstOrDefaultAsync() ?? null!;
            }
            if (_travellerRepository.Any(traveller => traveller.Id.Equals(id)))
            {
                return await _travellerRepository.GetMany(traveller => traveller.Id.Equals(id)).Select(traveller => new AuthViewModel
                {
                    Id = traveller.Id,
                    Role = UserRole.Traveller.ToString(),
                    Status = traveller.Account.Status
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
                    new Claim("status", model.Status.ToString()),
                }),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
