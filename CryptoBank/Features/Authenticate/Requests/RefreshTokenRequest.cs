using CryptoBank.Database;
using CryptoBank.Errors.Exceptions;
using CryptoBank.Features.Authenticate.Domain;
using CryptoBank.Features.Authenticate.Services;
using CryptoBank.Features.Management.Domain;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using static CryptoBank.Features.Authenticate.Errors.Codes.AuthenticateValidationErrors;

namespace CryptoBank.Features.Authenticate.Requests;

public static class RefreshTokenRequest 
{
    [HttpGet("/refreshToken")]
    [Authorize]
    public class Endpoint : EndpointWithoutRequest<Response>
    {
        private readonly Context _context;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenCookie _refreshTokenCookie;

        public Endpoint(
            Context context,
            ITokenService tokenService,
            IRefreshTokenCookie refreshTokenCookie)
        {
            _context = context;
            _tokenService = tokenService;
            _refreshTokenCookie = refreshTokenCookie;
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            var token = _refreshTokenCookie.GetRefreshTokenCookie()
                ?? throw new ValidationErrorsException(string.Empty, "Invalid token", InvalidToken);

            var (user, currentRefreshToken) = await CheckToken(token, cancellationToken);

            var accessToken = _tokenService.GetAccessToken(user);

            var newRefreshToken = _tokenService.GetRefreshToken();

            RefreshTokenRotation(currentRefreshToken, newRefreshToken.Token);

            _refreshTokenCookie.SetRefreshTokenCookie(newRefreshToken.Token);

            user.RefreshTokens.Add(newRefreshToken);

            _context.Update(user);

            await _context.SaveChangesAsync(cancellationToken);

            Response = new(accessToken);
        }

        private async Task<(User, RefreshToken)> CheckToken(string token, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .SingleAsync(x => x.RefreshTokens.Any(t => t.Token == token), cancellationToken);

            var currentRefreshToken = await _context.RefreshTokens
                .SingleAsync(x => x.Token == token, cancellationToken);

            if (currentRefreshToken.IsRevoked)
            {
                await _tokenService.RevokeRefreshTokens(currentRefreshToken.Token, cancellationToken);
            }

            if (!currentRefreshToken.IsActive)
            {
                throw new ValidationErrorsException(string.Empty, "Invalid token", InvalidToken);
            }

            return (user, currentRefreshToken);
        }

        private static RefreshToken RefreshTokenRotation(
            RefreshToken currentRefreshToken,
            string newRefreshToken)
        {
            currentRefreshToken.Revoked = DateTime.UtcNow;
            currentRefreshToken.ReasonRevoked = "Replaced token";
            currentRefreshToken.ReplacedByToken = newRefreshToken;
            currentRefreshToken.IsActive = false;
            currentRefreshToken.IsRevoked = true;

            return currentRefreshToken;
        }
    }

    public record Response(string AccessToken);
}