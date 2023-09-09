using CryptoBank.Database;
using CryptoBank.Errors.Exceptions;
using CryptoBank.Features.Authenticate.Domain;
using CryptoBank.Features.Authenticate.Models;
using CryptoBank.Features.Authenticate.Services;
using CryptoBank.Features.Management.Domain;
using CryptoBank.Pipeline;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using static CryptoBank.Features.Authenticate.Errors.Codes.AuthenticateValidationErrors;

namespace CryptoBank.Features.Authenticate.Requests;

public static class RefreshToken 
{
    [HttpGet("/refreshToken")]
    [AllowAnonymous]
    public class Endpoint : EndpointWithoutRequest<AuthenticateModel>
    {
        private readonly Dispatcher _dispatcher;
        private readonly IRefreshTokenCookie _refreshTokenCookie;

        public Endpoint(Dispatcher dispatcher, IRefreshTokenCookie refreshTokenCookie)
        {
            _dispatcher = dispatcher;
            _refreshTokenCookie = refreshTokenCookie;
        }

        public override async Task<AuthenticateModel> ExecuteAsync(CancellationToken cancellationToken)
        {
            var token = _refreshTokenCookie.GetRefreshTokenCookie();

            var response = await _dispatcher.Dispatch(new Request(token), cancellationToken);

            _refreshTokenCookie.SetRefreshTokenCookie(response.RefreshToken);

            return new AuthenticateModel { AccessToken = response.AccessToken };
        }
    }

    public record Request(string RefreshToken) : IRequest<Response>;

    public record Response(string AccessToken, string RefreshToken);

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly Context _context;
        private readonly ITokenService _tokenService;

        public RequestHandler(
            Context context,
            ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var (user, currentRefreshToken) = await CheckToken(request.RefreshToken, cancellationToken);

            var accessToken = _tokenService.GetAccessToken(user);

            var newRefreshToken = _tokenService.GetRefreshToken();

            RotateRefreshToken(currentRefreshToken, newRefreshToken.Token);

            user.UserTokens.Add(newRefreshToken);

            await _context.SaveChangesAsync(cancellationToken);

            return new Response(accessToken, newRefreshToken.Token);
        }

        private async Task<(User, UserToken)> CheckToken(string token, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .SingleAsync(x => x.UserTokens.Any(t => t.Token == token), cancellationToken);

            var currentRefreshToken = await _context.UserTokens
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

        private static UserToken RotateRefreshToken(
            UserToken currentRefreshToken,
            string newRefreshToken)
        {
            currentRefreshToken.Revoked = DateTime.UtcNow;
            currentRefreshToken.ReasonRevoked = "Replaced token";
            currentRefreshToken.ReplacedByToken = newRefreshToken;

            return currentRefreshToken;
        }
    }
}