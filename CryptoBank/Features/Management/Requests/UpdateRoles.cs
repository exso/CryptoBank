using CryptoBank.Authorization;
using CryptoBank.Database;
using CryptoBank.Errors.Exceptions;
using CryptoBank.Features.Management.Domain;
using CryptoBank.Pipeline;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Net;

using static CryptoBank.Features.Management.Errors.Codes.UserProfileValidationErrors;

namespace CryptoBank.Features.Management.Requests;

public static class UpdateRoles
{
    [Authorize(Policy = PolicyNames.AdministratorRole)]
    [HttpPost("/updateRoles")]
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

    public record Request(int UserId, int[] RoleIds) : IRequest<Unit>;

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleFor(x => x.RoleIds)
                .NotEmpty();
        }
    }

    public class RequestHandler : IRequestHandler<Request, Unit>
    {
        private readonly Context _context;

        public RequestHandler(Context context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .SingleOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

            if (user is null)
            {
                throw new ValidationErrorsException($"{nameof(user)}", "User not found", UserNotFound);
            }

            await RemoveOldRoles(user.Id, cancellationToken);

            await AddNewRoles(user, request.RoleIds, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        private async Task RemoveOldRoles(int userId, CancellationToken cancellationToken)
        {
            var rolesToRemove = await _context.UserRoles
                .Where(x => x.UserId == userId)
                .ToArrayAsync(cancellationToken);

            if (rolesToRemove.Any())
            {
                foreach (var role in rolesToRemove)
                {
                    _context.UserRoles.Remove(role);
                }
            }   
        }

        private async Task AddNewRoles(User user, int[] roleIds, CancellationToken cancellationToken)
        {
            var rolesToAdd = await _context.Roles
                .Where(x => roleIds.Contains(x.Id))
                .ToArrayAsync(cancellationToken);

            if (rolesToAdd.Any())
            {
                foreach (var role in rolesToAdd)
                {
                    user.UserRoles.Add(new UserRole
                    {
                        User = user,
                        Role = role
                    });
                }
            }
        }
    }
}
