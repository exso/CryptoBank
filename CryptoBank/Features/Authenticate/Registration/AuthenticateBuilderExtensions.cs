using CryptoBank.Features.Authenticate.Options;
using CryptoBank.Features.Authenticate.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CryptoBank.Features.Authenticate.Registration;

public static class AuthenticateBuilderExtensions
{
    public static WebApplicationBuilder AddAuthenticate(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
            var jwtOptions = builder.Configuration.GetSection("Features:Authenticate").Get<AuthenticateOptions>()!.Jwt;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
            };
        });

        builder.Services.Configure<AuthenticateOptions>(builder.Configuration.GetSection("Features:Authenticate"));

        builder.Services.AddTransient<ITokenService, TokenService>();

        builder.Services.AddTransient<IRefreshTokenCookie, RefreshTokenCookie>();

        builder.Services.AddHostedService<ArchivedRefreshTokensHostedService>();

        return builder;
    }
}