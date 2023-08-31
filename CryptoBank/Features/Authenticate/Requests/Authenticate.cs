using CryptoBank.Database;
using CryptoBank.Errors.Exceptions;
using CryptoBank.Features.Authenticate.Services;
using CryptoBank.Pipeline;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using static CryptoBank.Features.Authenticate.Errors.Codes.AuthenticateValidationErrors;

namespace CryptoBank.Features.Authenticate.Requests;

public static class Authenticate
{
    [HttpPost("/authenticate")]
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

    public record Request(string Email, string Password) : IRequest<Response>;

    public record Response(string AccessToken);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(Context context)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithErrorCode(EmailRequired)
                .MinimumLength(5)
                .MaximumLength(20)
                .EmailAddress().WithErrorCode(EmailInvalid)
                .MustAsync(async (email, cancellationToken) => await EmailExistsAsync(email, context, cancellationToken))
                .WithErrorCode(EmailNotFound);

            RuleFor(x => x.Password)
                .NotEmpty().WithErrorCode(PasswordRequired)
                .MinimumLength(5)
                .MaximumLength(20);
        }

        private static async Task<bool> EmailExistsAsync(string email, Context context, CancellationToken cancellationToken) =>
            await context.Users.AnyAsync(x => x.Email.Equals(email), cancellationToken);
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly Context _context;
        private readonly IAccessTokenService _accessTokenService;

        public RequestHandler(Context context, IAccessTokenService accessTokenService)
        {
            _accessTokenService = accessTokenService;
            _context = context;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .SingleAsync(x => x.Email == request.Email, cancellationToken);

            var hashPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);

            if (!hashPassword)
            {
                throw new ValidationErrorsException($"{nameof(request.Email)}", "Password invalid", PasswordInvalid);
            } 

            var token = _accessTokenService.GetAccessToken(user);

            return new Response(token);
        }
    }
}
