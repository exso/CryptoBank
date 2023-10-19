using CryptoBank.Common.Passwords;
using CryptoBank.Features.Management.Domain;
using CryptoBank.Features.Management.Requests;
using CryptoBank.Tests.Integration.Errors.Contracts;
using CryptoBank.Tests.Integration.Fixtures;
using CryptoBank.Tests.Integration.Helpers;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CryptoBank.Tests.Integration.Features.Management.Requests;

[Collection(UsersTestsCollection.Name)]
public class UpdateRolesTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;

    private AsyncServiceScope _scope;

    public UpdateRolesTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Should_update_roles()
    {
        // Arrange
        var userRole = new UserRole() { Role = new Role { Name = "Administrator", Description = "Администратор" } };

        var (client, user) = await _fixture.HttpClient.CreateAuthenticatedClient(Create.CancellationToken(), userRole);

        var role = new Role { Name = "Analyst", Description = "Аналитик" };

        await _fixture.Database.Execute(async x =>
        {
            x.Roles.Add(role);
            await x.SaveChangesAsync();
        });

        // Act
        var response = (await client.PostAsJsonAsync("/updateRoles", new
        {
            UserId = user.Id,
            RoleIds = new int[] { role.Id }
        })).EnsureSuccessStatusCode();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var roleContract = await _fixture.Database.Execute(async x =>
            await x.UserRoles
            .Include(x => x.User)
            .Include(x => x.Role)
            .SingleOrDefaultAsync(u => u.UserId == user.Id && u.RoleId == role.Id));

        roleContract.Should().NotBeNull();
        roleContract!.UserId.Should().Be(user.Id);
        roleContract!.RoleId.Should().Be(role.Id);
        roleContract.Role!.Name.Should().Be(role.Name);
        roleContract.Role!.Description.Should().Be(role.Description);
    }

    [Fact]
    public async Task Should_return_bad_request_if_user_not_found()
    {
        // Arrange
        var passwordHasher = _scope.ServiceProvider.GetRequiredService<Argon2IdPasswordHasher>();

        var user = UserHelper.CreateUser("me@example.com", passwordHasher.HashPassword("12345678"));
        user.UserRoles.Add(new UserRole() { Role = new Role { Name = "Administrator", Description = "Администратор" } });

        var role = new Role { Name = "Analyst", Description = "Аналитик" };

        await _fixture.Database.Execute(async x =>
        {
            x.Roles.Add(role);
            await x.SaveChangesAsync();
        });

        var jwt = AuthenticateHelper.GetAccessToken(user, _scope);

        var client = _fixture.HttpClient.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await client.PostAsJsonAsync("/updateRoles", new
        {
            UserId = 1,
            RoleIds = new int[] { role.Id }
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsContract>();
        validationProblemDetails.Should().NotBeNull();

        var error = validationProblemDetails!.Errors.Single();
        error.Code.Should().Be("user_profile_validation_user_not_found");
    }

    [Fact]
    public async Task Should_return_forbidden_if_wrong_role()
    {
        // Arrange
        var (client, user) = await _fixture.HttpClient.CreateAuthenticatedClient(Create.CancellationToken());

        // Act
        var response = await client.PostAsJsonAsync("/updateRoles", new
        {
            UserId = user.Id,
            RoleIds = new int[] { 1 }
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Should_forbid_if_invalid_token()
    {
        // Arrange
        var (client, user) = await _fixture.HttpClient.CreateWronglyAuthenticatedClient(Create.CancellationToken());

        // Act
        var response = await client.PostAsJsonAsync("/updateRoles", new
        {
            UserId = user.Id,
            RoleIds = new int[] { 1 }
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public async Task InitializeAsync()
    {
        await _fixture.Database.Clear(Create.CancellationToken());

        _scope = _fixture.Factory.Services.CreateAsyncScope();
    }

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
    }
}

public class UpdateRolesValidatorTests
{
    private readonly UpdateRoles.RequestValidator _validator = new();

    [Fact]
    public void Should_validate_correct_request()
    {
        var result = _validator.TestValidate(new UpdateRoles.Request(1, new int[] { 1 }));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Should_require_user_id()
    {
        var result = await _validator.TestValidateAsync(new UpdateRoles.Request(0, new int[] { 1 }));
        result.ShouldHaveValidationErrorFor(x => x.UserId).WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public async Task Should_require_role_ids()
    {
        var result = await _validator.TestValidateAsync(new UpdateRoles.Request(1, Array.Empty<int>()));
        result.ShouldHaveValidationErrorFor(x => x.RoleIds).WithErrorCode("NotEmptyValidator");
    }
}
