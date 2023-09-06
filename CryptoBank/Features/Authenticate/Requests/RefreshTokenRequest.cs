using CryptoBank.Database;
using CryptoBank.Errors.Exceptions;
using CryptoBank.Features.Authenticate.Domain;
using CryptoBank.Features.Authenticate.Services;
using CryptoBank.Features.Management.Domain;
using CryptoBank.Pipeline;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using static CryptoBank.Features.Authenticate.Errors.Codes.AuthenticateValidationErrors;

namespace CryptoBank.Features.Authenticate.Requests;

public static class RefreshTokenRequest
{
    [HttpPost("/refreshToken")]
    [AllowAnonymous]
    public class Endpoint : Endpoint<Request, Response>
    {
        private readonly Dispatcher _dispatcher;
        public Endpoint(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public override async Task<Response> ExecuteAsync(Request request, CancellationToken cancellationToken) =>
            await _dispatcher.Dispatch(request, cancellationToken);
    }

    public record Request(string AccessToken) : IRequest<Response>;
    public record Response(string AccessToken);

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly Context _context;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenCookie _refreshTokenCookie;

        public RequestHandler(
            Context context, 
            ITokenService tokenService,
            IRefreshTokenCookie refreshTokenCookie)
        {
            _context = context;
            _tokenService = tokenService;
            _refreshTokenCookie = refreshTokenCookie;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
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

            return new Response(accessToken);
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
                //TODO
                await _tokenService.RevokeRefreshTokens(user, currentRefreshToken.Token, cancellationToken);
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
}
