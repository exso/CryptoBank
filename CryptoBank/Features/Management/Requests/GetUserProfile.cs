using CryptoBank.Database;
using CryptoBank.Errors.Exceptions;
using CryptoBank.Features.Accounts.Services;
using CryptoBank.Pipeline;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using static CryptoBank.Features.Management.Errors.Codes.UserProfileValidationErrors;

namespace CryptoBank.Features.Management.Requests;

public static class GetUserProfile
{
    [HttpGet("/profile")]
    [Authorize]
    public class Endpoint : EndpointWithoutRequest<Response>
    {
        private readonly Dispatcher _dispatcher;
        private readonly UserIdentifierService _userIdentifierService;

        public Endpoint(Dispatcher dispatcher, UserIdentifierService userIdentifierService)
        {
            _dispatcher = dispatcher;
            _userIdentifierService = userIdentifierService;
        }

        public override async Task<Response> ExecuteAsync(CancellationToken cancellationToken)
        {
            var userId = _userIdentifierService.GetUserIdentifier();

            var response = await _dispatcher.Dispatch(new Request(userId), cancellationToken);

            return response;
        }
    }

    public record Request(int UserId) : IRequest<Response>;

    public record Response(
        int Id, 
        string Email, 
        DateTime DateOfBirth, 
        DateTime DateOfRegistration,
        string[] UserRoles);

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly Context _context;
        public RequestHandler(Context context)
        {
            _context = context;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .Where(x => x.Id == request.UserId)
                .Select(x => new Response(
                    x.Id, 
                    x.Email, 
                    x.DateOfBirth, 
                    x.DateOfRegistration, 
                    x.UserRoles.Select(x => x.Role.Name).ToArray()
                ))
                .SingleOrDefaultAsync(cancellationToken);

            return user is null
                ? throw new ValidationErrorsException($"{nameof(user)}", "User not found", UserNotFound)
                : user;
        }
    }
}