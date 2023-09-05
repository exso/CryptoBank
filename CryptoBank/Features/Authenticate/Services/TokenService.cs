using CryptoBank.Database;
using CryptoBank.Features.Authenticate.Domain;
using CryptoBank.Features.Authenticate.Options;
using CryptoBank.Features.Management.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CryptoBank.Features.Authenticate.Services;

public class TokenService : ITokenService
{
    private readonly AuthenticateOptions _options;
    private readonly Context _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenService(
        IOptions<AuthenticateOptions> options,
        Context context,
        IHttpContextAccessor httpContextAccessor)
    {
        _options = options.Value;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }
    public string GetAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, $"{user.Id}"),
            new(ClaimTypes.Email, user.Email)
        };

        foreach (var role in user.UserRoles)
        {
            claims.Add(new(ClaimTypes.Role, role.Role.Name));
        }
           
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Jwt.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.Add(_options.Jwt.AccessTokenExpiration);

        var token = new JwtSecurityToken(
            _options.Jwt.Issuer,
            _options.Jwt.Audience,
            claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GetRefreshToken()
    {
        var expires = DateTime.UtcNow.Add(_options.Jwt.RefreshTokenExpiration);

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new()
        {
            Token = token,
            Expires = expires,
            Created = DateTime.UtcNow,
            CreatedByIp = string.Empty,
            IsActive = true
        };
    }

    public async void RemoveArchiveRefreshTokens(User user, CancellationToken cancellationToken)
    {
        var expires = DateTime.UtcNow.Add(_options.Jwt.RefreshTokenArchiveExpiration);

        var refreshTokens = await _context.RefreshTokens
            .Where(x => !x.IsActive && x.Created <= expires)
            .ToArrayAsync(cancellationToken);

        if (refreshTokens.Any())
        {
            foreach (var token in refreshTokens)
            {
                user.RefreshTokens.Remove(token);
            }
        }       
    }

    public void SetRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.Add(_options.Jwt.RefreshTokenExpiration)
        };

        _httpContextAccessor.HttpContext!.Response.Cookies.Append("refresh-token", token, cookieOptions);
    }

    public string GetRefreshTokenCookie()
    {
        var token = _httpContextAccessor.HttpContext!.Request.Cookies["refresh-token"];

        return token!;
    } 
}
