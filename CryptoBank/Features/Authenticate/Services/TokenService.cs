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
        var expires = DateTime.UtcNow.Add(_options.Jwt.Expiration);

        var token = new JwtSecurityToken(
            _options.Jwt.Issuer,
            _options.Jwt.Audience,
            claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public UserToken GetRefreshToken()
    {
        var expires = DateTime.UtcNow.Add(_options.RefreshToken.Expiration);

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new()
        {
            Token = token,
            Expires = expires,
            Created = DateTime.UtcNow
        };
    }

    public async Task RemoveArchivedRefreshTokens(CancellationToken cancellationToken)
    {
        var expires = DateTime.UtcNow.Subtract(_options.RefreshToken.ArchiveExpiration);

        await _context.UserTokens
            .Where(x => x.Revoked != null && x.Created <= expires)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task RevokeRefreshTokens(int userId, CancellationToken cancellationToken)
    {
        await _context.UserTokens
            .Where(x => x.UserId == userId)
            .Where(x => x.Revoked == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Revoked, DateTime.UtcNow)
                .SetProperty(x => x.ReasonRevoked, "Invalid token"), cancellationToken);
    }
}
