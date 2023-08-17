using Data.Enums;
using Newtonsoft.Json;

namespace Application.Commons;

public static class RazorPageHelper
{
    public static UserSessionModel? GetUserFromSession(ISession session)
    {
        var userSessionData = session.GetString("User");
        return userSessionData is null
            ? null
            : JsonConvert.DeserializeObject<UserSessionModel>(userSessionData);
    }
}

public record UserSessionModel
{
    public Guid Id;
    public UserRole Role;
    public string Email = null!;
    public string FirstName = null!;
    public string LastName = null!;
}