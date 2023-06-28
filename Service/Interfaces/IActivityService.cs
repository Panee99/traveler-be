using Service.Models.Activity;
using Service.Models.Attendance;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IActivityService
{
    Task<Result<ActivityViewModel>> Create(ActivityCreateModel model);

    Task<Result> Delete(Guid id);
}