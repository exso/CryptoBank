using CryptoBank.Features.Authenticate.Domain;
using CryptoBank.Features.Management.Domain;

namespace CryptoBank.Features.Authenticate.Services;

public interface ITokenService
{
    string GetAccessToken(User user);
    RefreshToken GetRefreshToken();
    void RemoveArchiveRefreshTokens(User user, CancellationToken cancellationToken);
}
