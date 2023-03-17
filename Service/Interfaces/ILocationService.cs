using Service.Models.Location;
using Service.Results;

namespace Service.Interfaces;

public interface ILocationService
{
    Result<Guid> Create(LocationCreateModel model);
    Result<LocationViewModel> Find(Guid id);
}