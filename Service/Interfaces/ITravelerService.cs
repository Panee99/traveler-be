using Service.Models.Traveler;
using Service.Results;

namespace Service.Interfaces;

public interface ITravelerService
{
    Task<Result> Register(TravelerRegistrationModel model);

    Result<TravelerProfileViewModel> GetProfile(Guid id);
}