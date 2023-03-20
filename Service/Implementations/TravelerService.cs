using Data.EFCore;
using Data.Entities;
using Data.Enums;
using FirebaseAdmin.Auth;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Errors;
using Service.Interfaces;
using Service.Models.Traveler;
using Shared.Auth;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TravelerService : BaseService, ITravelerService
{
    private readonly ILogger<TravelerService> _logger;
    private readonly IMapper _mapper;

    public TravelerService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TravelerService> logger) : base(unitOfWork)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result> Register(TravelerRegistrationModel model)
    {
        if (!await _verifyIdToken(model.Phone, model.IdToken))
            return DomainErrors.Traveler.IdToken;

        _unitOfWork.Repo<Traveler>().Add(
            new Traveler
            {
                Phone = _formatPhoneNum(model.Phone),
                Password = AuthHelper.HashPassword(model.Password),
                Status = AccountStatus.ACTIVE,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender
            }
        );

        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<TravelerProfileViewModel>> GetProfile(Guid id)
    {
        var entity = await _unitOfWork.Repo<Traveler>()
            .Query()
            .FirstOrDefaultAsync(e => e.Id == id && e.Status == AccountStatus.ACTIVE);

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