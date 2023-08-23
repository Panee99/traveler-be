using Service.Models.Activity;
using Service.Models.TourGroup;
using Service.Models.User;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITourGroupService
{
    Task<Result> Delete(Guid groupId);
    
    Task<Result<TourGroupViewModel>> Get(Guid id);

    Task<int> CountTravelers(Guid tourGroupId);

    Task<Result> End(Guid id);

    Task<Result> SendEmergency(Guid tourGroupId, Guid senderId, EmergencyRequestModel model);
    
    Task<Result<List<UserViewModel>>> ListMembers(Guid tourGroupId);

    Task<Result<List<ActivityViewModel>>> ListActivities(Guid tourGroupId);
    
    Task<Result> UpdateCurrentSchedule(Guid tourGroupId, CurrentScheduleModel model);
}