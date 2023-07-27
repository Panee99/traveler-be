using Service.Models.Activity;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IActivityService
{
    Task<Result<string>> Create(PartialActivityModel model);

    Task<Result> Delete(Guid id);

    Task<Result> Update(PartialActivityModel model);
}