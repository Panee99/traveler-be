using Service.Models.Location;
using Service.Results;

namespace Service.Interfaces;

public interface ILocationService
{
    Task<Result<LocationViewModel>> Create(LocationCreateModel model);

    Task<Result> Update(Guid id, LocationUpdateModel model);

    Task<Result> Delete(Guid id);

    Task<Result<LocationViewModel>> Find(Guid id);
}