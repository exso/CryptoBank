namespace CryptoBank.Features.Authenticate.Services;

public interface IRefreshTokenCookie
{
    string GetRefreshTokenCookie();
    void SetRefreshTokenCookie(string refreshToken);
}
