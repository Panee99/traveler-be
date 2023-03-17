using Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace Data.Configurations;

public static class ConventionConfigurations
{
    public static void ConfigureEnums(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<AccountStatus>().HaveConversion<string>();
        configurationBuilder.Properties<PaymentStatus>().HaveConversion<string>();
        configurationBuilder.Properties<Gender>().HaveConversion<string>();
        configurationBuilder.Properties<VnPayRequestStatus>().HaveConversion<string>();
        configurationBuilder.Properties<TagType>().HaveConversion<string>();
    }
}