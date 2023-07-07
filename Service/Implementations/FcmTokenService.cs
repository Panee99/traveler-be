using Data.EFCore;
using Data.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.FcmToken;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class FcmTokenService : BaseService, IFcmTokenService
{
    public FcmTokenService(UnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<Result<FcmTokenViewModel>> Create(FcmTokenCreateModel model)
    {
        if (!await UnitOfWork.Users.AnyAsync(user => user.Id == model.UserId))
            return Error.NotFound(DomainErrors.User.NotFound);

        var existedTokens = await UnitOfWork.FcmTokens
            .Query()
            .Where(e => e.UserId == model.UserId)
            .Select(e => e.Token)
            .ToListAsync();

        if (existedTokens.Contains(model.Token))
            return Error.Conflict(DomainErrors.FcmToken.AlreadyExisted);

        var entity = model.Adapt<FcmToken>();
        UnitOfWork.FcmTokens.Add(entity);
        await UnitOfWork.SaveChangesAsync();

        return entity.Adapt<FcmTokenViewModel>();
    }

    public async Task<Result> Delete(Guid tokenId)
    {
        var token = await UnitOfWork.FcmTokens.FindAsync(tokenId);
        if (token is null) return Error.NotFound();

        UnitOfWork.FcmTokens.Remove(token);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<List<FcmTokenViewModel>>> FindTokens(List<Guid> userIds)
    {
        var tokens = await UnitOfWork.FcmTokens
            .Query()
            .Where(e => userIds.Contains(e.UserId))
            .ToListAsync();

        return tokens.Adapt<List<FcmTokenViewModel>>();
    }
}