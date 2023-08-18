using CryptoBank.Database;
using CryptoBank.Features.Management.Domain;
using CryptoBank.Features.Management.Models;
using CryptoBank.Features.Management.Options;
using CryptoBank.Pipeline;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoBank.Features.Management.Requests;

public static class RegisterUser
{
    [HttpPost("/register/user")]
    [AllowAnonymous]
    public class Endpoint : Endpoint<Request, UserModel[]>
    {
        private readonly Dispatcher _dispatcher;
        public Endpoint(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public override async Task<UserModel[]> ExecuteAsync(Request request, CancellationToken cancellationToken) =>
             await _dispatcher.Dispatch(request, cancellationToken);
    }

    public record Request(
        int Id, 
        string Email,
        string Password,
        DateTime DateOfBirth,
        DateTime DateOfRegistration,
        List<int> UserRoles) : IRequest<UserModel[]>;

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.Email)
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(20);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(20);
        }
    }

    public class RequestHandler : IRequestHandler<Request, UserModel[]>
    {
        private readonly Context _context;
        private readonly string _administratorEmail;
        public RequestHandler(Context context, IOptions<ManagmentOptions> options)
        {
            _context = context;
            _administratorEmail = options.Value.AdministratorEmail;
        }

        public async Task<UserModel[]> Handle(Request request, CancellationToken cancellationToken)
        {
            var currentUser = await _context.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .SingleOrDefaultAsync(x => x.Email.Contains(request.Email), cancellationToken);

            if (_administratorEmail.Contains(request.Email))
            {
                var user = ConvertToUser(request);

                _context.Users.Update(user);
                await _context.SaveChangesAsync(cancellationToken);

               

            }

            return new UserModel[] { };
        }

        private static User ConvertToUser(Request request)
        {
            return new User
            {
                Id = request.Id,
                Email = request.Email,
                Password = request.Password,
                DateOfBirth = request.DateOfBirth.ToUniversalTime(),
                DateOfRegistration = request.DateOfRegistration.ToUniversalTime(),
            };
        }
    }
}
