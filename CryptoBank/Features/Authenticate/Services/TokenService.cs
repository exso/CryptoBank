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

    public TokenService(
        IOptions<AuthenticateOptions> options,
        Context context)
    {
        _options = options.Value;
        _context = context;
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

    public async Task RemoveArchivedRefreshTokens(CancellationToken cancellationToken)
    {
        var refreshTokens = await _context.RefreshTokens
            .Where(x => !x.IsActive && x.Created.AddDays(_options.Jwt.RefreshTokenArchiveExpiration) <= DateTime.UtcNow)
            .ToArrayAsync(cancellationToken);

        if (refreshTokens.Any())
        {
            _context.RefreshTokens.RemoveRange(refreshTokens);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokeRefreshTokens(string refreshToken, CancellationToken cancellationToken)
    {
        var refreshTokens = await _context.RefreshTokens
            .Where(x => x.Token == refreshToken || x.ReplacedByToken == refreshToken)
            .ToArrayAsync(cancellationToken);

        if (refreshTokens.Any())
        {
            foreach (var token in refreshTokens)
            {
                token.ReasonRevoked = "Invalid token";

                _context.RefreshTokens.Update(token);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
