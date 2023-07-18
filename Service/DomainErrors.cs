namespace Service;

public static class DomainErrors
{
    public static class Auth
    {
        public const string LoginFailed = "Login failed, wrong username or password";
        public const string NotSupportedRole = "Not supported Role";
    }

    public static class Activity
    {
        public const string NotFound = "Activity not found";
    }

    public static class User
    {
        public const string NotFound = "User not found";
        public const string PhoneExisted = "User with this phone number already exist";
        public const string NoCurrentGroup = "No current tour group joined";
    }

    public static class FcmToken
    {
        public const string AlreadyExisted = "Token already existed";
    }

    public static class Tour
    {
        public const string NotFound = "Tour not found";
    }

    public static class TourGroup
    {
        public const string NotFound = "Tour Group not found";
    }

    public static class TourGuide
    {
        public const string NotFound = "Tour Guide not found";
    }

    public static class Schedule
    {
        public const string NotFound = "Schedule not found";
    }
    
    public static class Trip
    {
        public const string NotFound = "Trip not found";
    }
}