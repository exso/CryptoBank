using CryptoBank.Database;
using CryptoBank.Errors.Exceptions;
using CryptoBank.Features.Authenticate.Services;
using CryptoBank.Pipeline;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Net;

using static CryptoBank.Features.Authenticate.Errors.Codes.AuthenticateValidationErrors;

namespace CryptoBank.Features.Authenticate.Requests;

public static class RevokeTokenRequest
{
    [HttpPost("/revokeToken")]
    [AllowAnonymous]
    public class Endpoint : Endpoint<Request, HttpStatusCode>
    {
        private readonly Dispatcher _dispatcher;
        public Endpoint(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public override async Task<HttpStatusCode> ExecuteAsync(Request request, CancellationToken cancellationToken)
        {
            await _dispatcher.Dispatch(request, cancellationToken);

            return HttpStatusCode.OK;
        }         
    }

    public record Request(string AccessToken) : IRequest<Unit>;

    public class RequestHandler : IRequestHandler<Request, Unit>
    {
        private readonly Context _context;
        private readonly IRefreshTokenCookie _refreshTokenCookie;

        public RequestHandler(
            Context context,
            IRefreshTokenCookie refreshTokenCookie)
        {
            _context = context;
            _refreshTokenCookie = refreshTokenCookie;
        }

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var token = _refreshTokenCookie.GetRefreshTokenCookie()
                ?? throw new ValidationErrorsException(string.Empty, "Invalid token", InvalidToken);

            var currentRefreshToken = await _context.RefreshTokens
                .SingleAsync(x => x.Token == token, cancellationToken);

            if (!currentRefreshToken.IsActive)
            {
                throw new ValidationErrorsException(string.Empty, "Invalid token", InvalidToken);
            }

            currentRefreshToken.Revoked = DateTime.UtcNow;
            currentRefreshToken.ReasonRevoked = "Revoked token";
            currentRefreshToken.IsRevoked = true;
            currentRefreshToken.IsActive = false;

            _context.RefreshTokens.Update(currentRefreshToken);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
