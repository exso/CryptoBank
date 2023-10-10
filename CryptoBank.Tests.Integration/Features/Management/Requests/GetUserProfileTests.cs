using CryptoBank.Common.Passwords;
using CryptoBank.Features.Management.Requests;
using CryptoBank.Tests.Integration.Errors.Contracts;
using CryptoBank.Tests.Integration.Fixtures;
using CryptoBank.Tests.Integration.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CryptoBank.Tests.Integration.Features.Management.Requests;

[Collection(UsersTestsCollection.Name)]
public class GetUserProfileTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;

    private AsyncServiceScope _scope;

    public GetUserProfileTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Should_get_user_profile()
    {
        // Arrange
        var passwordHasher = _scope.ServiceProvider.GetRequiredService<Argon2IdPasswordHasher>();

        var user = UserHelper.CreateUser("me@example.com", passwordHasher.HashPassword("12345678"));

        await _fixture.Database.Execute(async x =>
        {
            x.Users.Add(user);
            await x.SaveChangesAsync();
        });

        var jwt = AuthenticateHelper.GetAccessToken(user, _scope);

        var client = _fixture.HttpClient.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await client.GetFromJsonAsync<GetUserProfile.Response>("/profile");

        // Assert
        response.Should().NotBeNull();
        response!.Id.Should().Be(user.Id);
        response.Email.Should().Be(user.Email);
        response.DateOfBirth.Date.Should().Be(user.DateOfBirth.Date);
        response.DateOfRegistration.Date.Should().Be(user.DateOfRegistration.Date);
    }

    [Fact]
    public async Task Should_invalid_token()
    {
        // Arrange
        var jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwia";

        var client = _fixture.HttpClient.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await client.GetAsync("/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_user_not_found()
    {
        // Arrange
        var passwordHasher = _scope.ServiceProvider.GetRequiredService<Argon2IdPasswordHasher>();

        var user = UserHelper.CreateUser("me@example.com", passwordHasher.HashPassword("12345678"));

        var jwt = AuthenticateHelper.GetAccessToken(user, _scope);

        var client = _fixture.HttpClient.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await client.GetAsync("/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsContract>();
        validationProblemDetails.Should().NotBeNull();

        var error = validationProblemDetails!.Errors.Single();
        error.Code.Should().Be("user_profile_validation_user_not_found");
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
