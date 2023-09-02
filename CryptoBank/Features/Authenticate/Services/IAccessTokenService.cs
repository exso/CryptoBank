using CryptoBank.Features.Management.Domain;

namespace CryptoBank.Features.Authenticate.Services;

public interface IAccessTokenService
{
    string GetAccessToken(User user);
}
