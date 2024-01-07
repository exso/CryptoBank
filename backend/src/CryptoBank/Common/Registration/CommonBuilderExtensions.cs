using CryptoBank.Common.Passwords;
using CryptoBank.Common.Services;

namespace CryptoBank.Common.Registration;

public static class CommonBuilderExtensions
{
    public static WebApplicationBuilder AddCommon(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<Argon2IdPasswordHasher>();

        builder.Services.Configure<Argon2IdOptions>(builder.Configuration.GetSection("Common:Passwords:Argon2Id"));

        builder.Services.AddTransient<UserIdentifierService>();

        return builder;
    }
}
