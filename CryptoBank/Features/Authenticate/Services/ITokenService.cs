using CryptoBank.Features.Authenticate.Domain;
using CryptoBank.Features.Management.Domain;

namespace CryptoBank.Features.Authenticate.Services;

public interface ITokenService
{
    string GetAccessToken(User user);
    UserToken GetRefreshToken();
    Task RevokeRefreshTokens(string refreshToken, CancellationToken cancellationToken);
    Task RemoveArchivedRefreshTokens(CancellationToken cancellationToken);
}
