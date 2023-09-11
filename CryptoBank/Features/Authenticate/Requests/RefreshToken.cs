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
            var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

            var user = await FindUser(request.RefreshToken, cancellationToken);

            await CheckRefreshToken(user.Id, request.RefreshToken, cancellationToken);

            var accessToken = _tokenService.GetAccessToken(user);

            var newRefreshToken = _tokenService.GetRefreshToken();

            await SaveRefreshToken(user, newRefreshToken, cancellationToken);

            await RotateRefreshToken(request.RefreshToken, newRefreshToken.Id);

            await tx.CommitAsync(cancellationToken);

            return new Response(accessToken, newRefreshToken.Token);
        }

        private async Task<User> FindUser(string refreshToken, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .SingleAsync(x => x.UserTokens.Any(t => t.Token == refreshToken), cancellationToken);

            return user;
        }

        private async Task CheckRefreshToken(int userId, string refreshToken, CancellationToken cancellationToken)
        {
            var currentRefreshToken = await _context.UserTokens
                .SingleAsync(x => x.Token == refreshToken, cancellationToken);

            if (currentRefreshToken.IsRevoked)
            {
                await _tokenService.RevokeRefreshTokens(userId, cancellationToken);
            }

            if (!currentRefreshToken.IsActive)
            {
                throw new ValidationErrorsException(string.Empty, "Invalid token", InvalidToken);
            }

            if (currentRefreshToken.Expires <= DateTime.UtcNow)
            {
                throw new ValidationErrorsException(string.Empty, "Invalid token", InvalidToken);
            }
        }

        private async Task SaveRefreshToken(
            User user, 
            UserToken newRefreshToken, 
            CancellationToken cancellationToken)
        {
            user.UserTokens.Add(newRefreshToken);

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task RotateRefreshToken(
            string refreshToken,
            int newRefreshTokenId)
        {
            await _context.UserTokens
                .Where(x => x.Token == refreshToken)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Revoked, DateTime.UtcNow)
                    .SetProperty(x => x.ReasonRevoked, "Replaced token")
                    .SetProperty(x => x.ReplacedByTokenId, newRefreshTokenId));
        }
    }
}