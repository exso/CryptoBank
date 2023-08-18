using CryptoBank.Features.Management.Options;

namespace CryptoBank.Features.Management.Registration;

public static class ManagmentBuilderExtensions
{
    public static WebApplicationBuilder AddManagement(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<ManagmentOptions>(builder.Configuration.GetSection("Features:Management"));

        return builder;
    }
}
