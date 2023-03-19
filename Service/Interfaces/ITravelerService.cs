using Service.Models.Traveler;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITravelerService
{
    Task<Result> Register(TravelerRegistrationModel model);

    Result<TravelerProfileViewModel> GetProfile(Guid id);
}