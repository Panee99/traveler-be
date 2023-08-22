using Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace Data.EFCore.Configurations;

public static class ConventionConfigurations
{
    public static void ConfigureEnums(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<UserRole>().HaveConversion<string>();
        configurationBuilder.Properties<UserStatus>().HaveConversion<string>();
        configurationBuilder.Properties<Gender>().HaveConversion<string>();
        configurationBuilder.Properties<TourGroupStatus>().HaveConversion<string>();
        configurationBuilder.Properties<TourType>().HaveConversion<string>();
        configurationBuilder.Properties<Vehicle>().HaveConversion<string>();
        configurationBuilder.Properties<NotificationType>().HaveConversion<string>();
        configurationBuilder.Properties<ActivityType>().HaveConversion<string>();
    }
}