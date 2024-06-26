﻿using CryptoBank.Common.Passwords;
using CryptoBank.Database;
using CryptoBank.Features.Management.Domain;
using CryptoBank.Features.Management.Options;
using CryptoBank.Pipeline;
using CryptoBank.Validation;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;

using static CryptoBank.Features.Management.Errors.Codes.UserProfileValidationErrors;

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

        public override async Task<HttpStatusCode> ExecuteAsync(Request request, CancellationToken cancellationToken)
        {
            await _dispatcher.Dispatch(request, cancellationToken);

            return HttpStatusCode.OK;
        }            
    }

    public record Request(
        string Email,
        string Password,
        DateTime DateOfBirth) : IRequest<Unit>
    {
        public string LowercaseEmail => Email.ToLower();
    }

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(Context context)
        {
            RuleFor(x => x.LowercaseEmail)
                .NotEmpty().WithErrorCode(EmailRequired)
                .ValidLength()
                .EmailAddress().WithErrorCode(InvalidСredentials)
                .MustAsync(async (x, cancellationToken) => !await BeUniqueAsync(x, context, cancellationToken))
                .WithErrorCode(InvalidСredentials);

            RuleFor(x => x.Password)
                .NotEmpty().WithErrorCode(PasswordRequired)
                .ValidLength();
        }

        private static async Task<bool> BeUniqueAsync(string email, Context context, CancellationToken cancellationToken)
        {
            var bUnique = await context.Users
                .AnyAsync(x => x.Email.Equals(email), cancellationToken);

            return bUnique;
        }
    }

    public class RequestHandler : IRequestHandler<Request, Unit>
    {
        private readonly Context _context;
        private readonly ManagementOptions _managmentOptions;
        private readonly Argon2IdPasswordHasher _passwordHasher;
        public RequestHandler(
            Context context, 
            IOptions<ManagementOptions> managmentOptions,
            Argon2IdPasswordHasher passwordHasher)
        {
            _context = context;
            _managmentOptions = managmentOptions.Value;
            _passwordHasher = passwordHasher;
        }

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            //1. Определяем роль для пользователя
            var roleName = await DefineRole(request.LowercaseEmail, cancellationToken);

            //2. Получаем роль
            var role = await FindRole(roleName, cancellationToken);

            //3. Регистрируем пользователя
            await SaveUser(request, role, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        private async Task<string> DefineRole(string email, CancellationToken cancellationToken)
        {
            var existingAdmin = await _context.UserRoles
                .AnyAsync(x => x.Role!.Name.Equals(Roles.Administrator), cancellationToken);

            if (!existingAdmin && _managmentOptions.AdministratorEmail.Contains(email))
            {
                return Roles.Administrator;
            }

            return Roles.User;
        }

        private async Task<Role> FindRole(string roleName, CancellationToken cancellationToken)
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(x => x.Name.Contains(roleName), cancellationToken);

            return role!;
        }
       
        private async Task<Unit> SaveUser(Request request, Role role, CancellationToken cancellationToken)
        {
            var passwordHash = _passwordHasher.HashPassword(request.Password);

            var user = ConvertToUser(request, role, passwordHash);

            await _context.Users.AddAsync(user, cancellationToken);

            return Unit.Value;
        }
  
        private static User ConvertToUser(Request request, Role role, string passwordHash) =>
            new()
            {
                Email = request.LowercaseEmail,
                Password = passwordHash,
                DateOfBirth = request.DateOfBirth.ToUniversalTime(),
                DateOfRegistration = DateTime.UtcNow,
                UserRoles = new List<UserRole>()
                {
                    new()
                    {
                        Role = role,
                    }
                }              
            };
    }
}
