using Data.Entities.Activities;
using Service.Models.Activity;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IActivityService
{
    Task<Result> Create(ActivityCreateModel model);

    Task<Result> Delete(Guid id);

    Task<Result> Update(AttendanceActivity model);

    Task<Result> Update(CustomActivity model);


    Task<Result> Update(IncurredCostActivity model);


    Task<Result> Update(NextDestinationActivity model);
}