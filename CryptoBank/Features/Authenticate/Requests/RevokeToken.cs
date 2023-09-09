using CryptoBank.Authorization;
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

public static class RevokeToken
{
    [Authorize(Policy = PolicyNames.AdministratorRole)]
    [HttpPost("/revokeToken")]
    public class Endpoint : EndpointWithoutRequest<HttpStatusCode>
    {
        private readonly Dispatcher _dispatcher;
        private readonly IRefreshTokenCookie _refreshTokenCookie;

        public Endpoint(Dispatcher dispatcher, IRefreshTokenCookie refreshTokenCookie)
        {
            _dispatcher = dispatcher;
            _refreshTokenCookie = refreshTokenCookie;
        }

        public override async Task<HttpStatusCode> ExecuteAsync(CancellationToken cancellationToken)
        {
            var token = _refreshTokenCookie.GetRefreshTokenCookie();

            await _dispatcher.Dispatch(new Request(token), cancellationToken);

            return HttpStatusCode.OK;
        }
    }

    public record Request(string RefreshToken) : IRequest<Unit>;

    public class RequestHandler : IRequestHandler<Request, Unit>
    {
        private readonly Context _context;

        public RequestHandler(Context context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var currentRefreshToken = await _context.UserTokens
                .SingleAsync(x => x.Token == request.RefreshToken, cancellationToken);

            if (!currentRefreshToken.IsActive)
            {
                throw new ValidationErrorsException(string.Empty, "Invalid token", InvalidToken);
            }

            currentRefreshToken.Revoked = DateTime.UtcNow;
            currentRefreshToken.ReasonRevoked = "Revoked token";

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
