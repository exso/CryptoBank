using CryptoBank.Common.Passwords;
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

    public record Request(string Email, string Password) : IRequest<Response>
    {
        public string LowercaseEmail => Email.ToLower();
    }

    public record Response(string AccessToken);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(Context context)
        {
            RuleFor(x => x.LowercaseEmail)
                .NotEmpty().WithErrorCode(EmailRequired)
                .MinimumLength(5)
                .MaximumLength(20)
                .EmailAddress().WithErrorCode(InvalidСredentials)
                .MustAsync(async (email, cancellationToken) => await EmailExistsAsync(email, context, cancellationToken))
                .WithErrorCode(InvalidСredentials);

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
        private readonly ITokenService _tokenService;
        private readonly Argon2IdPasswordHasher _passwordHasher;

        public RequestHandler(
            Context context, 
            ITokenService tokenService,
            Argon2IdPasswordHasher passwordHasher)
        {
            _tokenService = tokenService;
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .SingleAsync(x => x.Email == request.LowercaseEmail, cancellationToken);

            var passwordHash = _passwordHasher.VerifyHashedPassword(user.Password, request.Password);

            if (!passwordHash)
            {
                throw new ValidationErrorsException($"{nameof(request.LowercaseEmail)}", "Invalid credentials", InvalidСredentials);
            } 

            var accessToken = _tokenService.GetAccessToken(user);

            var refreshToken = _tokenService.GetRefreshToken();

            user.RefreshTokens.Add(refreshToken);

            _tokenService.RemoveArchiveRefreshTokens(user, cancellationToken);

            _context.Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response(accessToken);
        }
    }
}
