using CryptoBank.Features.Authenticate.Services;

namespace CryptoBank.Features.Authenticate.Registration;

public static class AuthenticateBuilderExtensions
{
    public static WebApplicationBuilder AddAuthenticate(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<IAccessTokenService, AccessTokenService>();

        return builder;
    }
}