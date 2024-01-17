using CryptoBank.Features.Authenticate.Services;
using CryptoBank.Features.Management.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoBank.Tests.Integration.Helpers;

public static class AuthenticateHelper
{
    public static string GetAccessToken(User user, AsyncServiceScope scope)
    {
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        var accessToken = tokenService.GetAccessToken(user);

        return accessToken;
    }
}
