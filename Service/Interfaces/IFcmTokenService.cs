using Service.Models.FcmToken;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IFcmTokenService
{
    Task<Result<FcmTokenViewModel>> Create(FcmTokenCreateModel model);

    Task<Result> Delete(Guid tokenId);

    Task<Result<List<FcmTokenViewModel>>> FindTokens(List<Guid> userIds);
}