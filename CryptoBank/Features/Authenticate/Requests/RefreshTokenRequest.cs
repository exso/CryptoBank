using CryptoBank.Database;
using CryptoBank.Features.Authenticate.Domain;
using CryptoBank.Features.Authenticate.Services;
using CryptoBank.Pipeline;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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

        public RequestHandler(Context context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var token = _tokenService.GetRefreshTokenCookie();

            if (token == null)
            {
                //токен может быть пустым
            }

            var user = await _context.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .SingleAsync(x => x.RefreshTokens.Any(t => t.Token == token), cancellationToken);

            var currentRefreshToken = await _context.RefreshTokens
                .SingleAsync(x => x.Token == token, cancellationToken);

            if (currentRefreshToken.IsRevoked)
            {
               
            }

            if (!currentRefreshToken.IsActive)
            {

            }

            var accessToken = _tokenService.GetAccessToken(user);

            var newRefreshToken = _tokenService.GetRefreshToken();

            RefreshTokenRotation(currentRefreshToken, newRefreshToken.Token);

            _tokenService.SetRefreshTokenCookie(newRefreshToken.Token);

            await _tokenService.AddAndRemoveRefreshTokens(user, newRefreshToken, cancellationToken);

            return new Response(accessToken);
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
