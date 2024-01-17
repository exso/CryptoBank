using CryptoBank.Errors.Exceptions;
using CryptoBank.Features.Authenticate.Options;
using Microsoft.Extensions.Options;

using static CryptoBank.Features.Authenticate.Errors.Codes.AuthenticateValidationErrors;

namespace CryptoBank.Features.Authenticate.Services;

public class RefreshTokenCookie : IRefreshTokenCookie
{
    private readonly AuthenticateOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CookieName = "refresh-token";

    public RefreshTokenCookie(
        IOptions<AuthenticateOptions> options,
        IHttpContextAccessor httpContextAccessor)
    {
        _options = options.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.Add(_options.RefreshToken.Expiration)
        };

        _httpContextAccessor.HttpContext!.Response.Cookies.Append(CookieName, refreshToken, cookieOptions);
    }

    public string GetRefreshTokenCookie()
    {
        var refreshToken = _httpContextAccessor.HttpContext!.Request.Cookies[CookieName]
            ?? throw new ValidationErrorsException(string.Empty, "Invalid token", InvalidToken);

        return refreshToken;
    }
}
