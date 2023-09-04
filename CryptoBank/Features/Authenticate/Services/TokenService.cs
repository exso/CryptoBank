using CryptoBank.Features.Authenticate.Options;
using CryptoBank.Features.Management.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CryptoBank.Features.Authenticate.Services;

public class TokenService : ITokenService
{
    private readonly AuthenticateOptions _options;

    public TokenService(IOptions<AuthenticateOptions> options)
    {
        _options = options.Value;
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
        var expires = DateTime.Now + _options.Jwt.Expiration;

        var token = new JwtSecurityToken(
            _options.Jwt.Issuer,
            _options.Jwt.Audience,
            claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
