using CryptoBank.Features.Authenticate.Domain;
using CryptoBank.Features.Management.Domain;

namespace CryptoBank.Features.Authenticate.Services;

public interface ITokenService
{
    string GetAccessToken(User user);
    RefreshToken GetRefreshToken();
    Task RevokeRefreshTokens(User user, string refreshToken, CancellationToken cancellationToken);
    Task RemoveArchivedRefreshTokens(CancellationToken cancellationToken);
}
