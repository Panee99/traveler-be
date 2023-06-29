using Service.Models.Activity;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IActivityService
{
    Task<Result<ActivityViewModel>> Create(ActivityCreateModel model);
    
    Task<Result> Delete(Guid id);
    
    Task<Result<ActivityViewModel>> Update(Guid activityId, ActivityUpdateModel model);
}