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
using System.Net;

namespace CryptoBank.Features.Management.Requests;

public static class RegisterUser
{
    [HttpPost("/register/user")]
    [AllowAnonymous]
    public class Endpoint : Endpoint<Request, HttpStatusCode>
    {
        private readonly Dispatcher _dispatcher;
        public Endpoint(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public override async Task<HttpStatusCode> ExecuteAsync(Request request, CancellationToken cancellationToken) =>
             await _dispatcher.Dispatch(request, cancellationToken);
    }

    public record Request(
        int Id, 
        string Email,
        string Password,
        DateTime DateOfBirth,
        DateTime DateOfRegistration) : IRequest<HttpStatusCode>;

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(Context context)
        {
            RuleFor(x => x.Id)
                .GreaterThanOrEqualTo(0);
            
            RuleFor(x => x.Email)
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(20)
                .Must(x => !BeUnique(x, context))
                .WithMessage(x => "Email duplicate");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(20);
        }

        private static bool BeUnique(string email, Context context)
        {
            var bUnique = context.Users
                .Any(x => x.Email.Equals(email));

            return bUnique;
        }
    }

    public class RequestHandler : IRequestHandler<Request, HttpStatusCode>
    {
        private readonly Context _context;
        private readonly ManagmentOptions _managmentOptions;
        public RequestHandler(Context context, IOptions<ManagmentOptions> managmentOptions)
        {
            _context = context;
            _managmentOptions = managmentOptions.Value;
        }

        public async Task<HttpStatusCode> Handle(Request request, CancellationToken cancellationToken)
        {
            //1. Регистрируем пользователя
            var userId = await SaveUser(request, cancellationToken);

            //2. Определяем роль для пользователя
            var roleName = await DefinitionRole(request, cancellationToken);

            //3. Получаем роль
            var roleId = await FindRole(roleName, cancellationToken);

            //4. Назначаем роль пользователю
            await SaveUserRoles(userId, roleId, cancellationToken);

            return HttpStatusCode.OK;
        }

        private async Task<string> DefinitionRole(Request request, CancellationToken cancellationToken)
        {
            var existingAdmin = await _context.UserRoles
                .AnyAsync(x => x.Role.Name.Equals(Roles.Administrator), cancellationToken);

            if (!existingAdmin && _managmentOptions.AdministratorEmail.Contains(request.Email))
            {
                return Roles.Administrator;
            }
            else
            {
                return Roles.User;
            }
        }

        private async Task<int> FindRole(string roleName, CancellationToken cancellationToken) =>
            await _context.Roles
                .Where(x => x.Name.Contains(roleName))
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);


        private async Task<int> SaveUser(Request request, CancellationToken cancellationToken)
        {
            var user = ConvertToUser(request);

            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return user.Id;
        }

        private async Task SaveUserRoles(int userId, int roleId, CancellationToken cancellationToken)
        {
            var userRoles = ConvertToUserRoles(userId, roleId);

            await _context.UserRoles.AddAsync(userRoles, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        private static UserRole ConvertToUserRoles(int userId, int roleId) =>
            new()
            {
                RoleId = roleId,
                UserId = userId
            };
            
        private static User ConvertToUser(Request request) =>
            new()
            {
                Id = request.Id,
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                DateOfBirth = request.DateOfBirth.ToUniversalTime(),
                DateOfRegistration = request.DateOfRegistration.ToUniversalTime(),
            };
    }
}
