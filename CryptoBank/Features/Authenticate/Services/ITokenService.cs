using CryptoBank.Features.Authenticate.Domain;
using CryptoBank.Features.Management.Domain;

namespace CryptoBank.Features.Authenticate.Services;

public interface ITokenService
{
    string GetAccessToken(User user);
    RefreshToken GetRefreshToken();
    void RemoveArchiveRefreshTokens(User user, CancellationToken cancellationToken);
    void SetRefreshTokenCookie(string token);
    string GetRefreshTokenCookie();
    Task AddAndRemoveRefreshTokens(User user, RefreshToken refreshToken, CancellationToken cancellationToken);
}
