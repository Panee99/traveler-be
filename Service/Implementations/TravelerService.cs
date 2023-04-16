using Data.EFCore;
using Data.EFCore.Repositories;
using Data.Entities;
using Data.Enums;
using FirebaseAdmin.Auth;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Errors;
using Service.Interfaces;
using Service.Models.Traveler;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TravelerService : BaseService, ITravelerService
{
    private readonly ILogger<TravelerService> _logger;
    private readonly IMapper _mapper;

    //
    private readonly IRepository<Account> _accountRepo;
    private readonly IRepository<Traveler> _travelerRepo;

    public TravelerService(IUnitOfWork unitOfWork, IMapper mapper,
        ILogger<TravelerService> logger, IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, httpContextAccessor)
    {
        _mapper = mapper;
        _logger = logger;
        //
        _accountRepo = unitOfWork.Repo<Account>();
        _travelerRepo = unitOfWork.Repo<Traveler>();
    }

    public async Task<Result> Register(TravelerRegistrationModel model)
    {
        if (!model.Phone.StartsWith('+')) model.Phone = '+' + model.Phone;

        if (!model.Phone.StartsWith("+84")) return Error.Validation();

        if (AuthUser is not { Role: AccountRole.Manager })
            if (model.IdToken is null || !await _verifyIdToken(model.Phone, model.IdToken))
                return DomainErrors.Traveler.IdToken;

        var formattedPhone = _formatPhoneNum(model.Phone);

        if (await _accountRepo.AnyAsync(e => e.Phone == formattedPhone))
            return Error.Conflict("Account with this phone number already exist");

        _travelerRepo.Add(
            new Traveler
            {
                Phone = formattedPhone,
                Password = AuthHelper.HashPassword(model.Password),
                Status = AccountStatus.Active,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                Role = AccountRole.Traveler
            }
        );

        await UnitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<TravelerProfileViewModel>> GetProfile(Guid id)
    {
        var entity = await _travelerRepo
            .Query()
            .FirstOrDefaultAsync(e => e.Id == id && e.Status == AccountStatus.Active);

        if (entity is null) return Error.NotFound();
        return _mapper.Map<TravelerProfileViewModel>(entity);
    }


    // PRIVATE

    #region PRIVATE

    private string _formatPhoneNum(string phone)
    {
        if (phone.StartsWith('+')) phone = phone.Substring(1);
        return phone;
    }

    private async Task<bool> _verifyIdToken(string phone, string idToken)
    {
        try
        {
            var firebaseToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            var tokenPhone = firebaseToken.Claims["phone_number"];
            if (phone.Equals(tokenPhone)) return true;
        }
        catch (Exception e)
        {
            _logger.LogDebug(e, "{Message}", typeof(TravelerService).ToString());
        }

        return false;
    }

    #endregion
}