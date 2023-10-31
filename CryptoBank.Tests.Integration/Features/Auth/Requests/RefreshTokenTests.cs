using CryptoBank.Common.Passwords;
using CryptoBank.Features.Authenticate.Domain;
using CryptoBank.Features.Authenticate.Models;
using CryptoBank.Tests.Integration.Errors.Contracts;
using CryptoBank.Tests.Integration.Fixtures;
using CryptoBank.Tests.Integration.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace CryptoBank.Tests.Integration.Features.Auth.Requests;

[Collection(AuthTestsCollection.Name)]
public class RefreshTokenTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;

    private AsyncServiceScope _scope;

    public RefreshTokenTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Should_refresh_token()
    {
        // Arrange
        var passwordHasher = _scope.ServiceProvider.GetRequiredService<Argon2IdPasswordHasher>();

        var user = UserHelper.CreateUser("test@test.com", passwordHasher.HashPassword("qwerty123456A!"));

        await _fixture.Database.Execute(async x =>
        {
            x.Users.Add(user);
            await x.SaveChangesAsync();
        });

        var client = _fixture.HttpClient.CreateClient();

        var authResponse = await client.PostAsJsonAsync<AuthenticateModel>("/authenticate", new
        {
            Email = "test@test.com",
            Password = "qwerty123456A!",
        });

        authResponse.Should().NotBeNull();
        authResponse.AccessToken.Should().NotBeNullOrEmpty();

        // Act
        var response = (await client.GetAsync("/refreshToken"))
            .EnsureSuccessStatusCode();

        // Assert
        var authenticateModel = await response.Content.ReadFromJsonAsync<AuthenticateModel>();
        authenticateModel.Should().NotBeNull();
        authenticateModel!.AccessToken.Should().NotBeNullOrEmpty();

        string cookieHeader = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value.First();
        string refreshToken = cookieHeader.Split(';').First().Trim()["refresh-token=".Length..];
        string refreshTokenDecode = Uri.UnescapeDataString(refreshToken);
        refreshTokenDecode.Should().NotBeNullOrEmpty();

        var userToken = await _fixture.Database.Execute(async x =>
            await x.UserTokens.SingleOrDefaultAsync(u => u.Token == refreshTokenDecode));

        userToken.Should().NotBeNull();
        userToken!.UserId.Should().Be(user.Id);

        var oldToken = await _fixture.Database.Execute(async x =>
            await x.UserTokens.SingleOrDefaultAsync(u => u.ReplacedByTokenId == userToken.Id));

        oldToken!.IsActive.Should().BeFalse();
        oldToken.Revoked.Should().NotBeNull();
        oldToken.ReasonRevoked.Should().Be("Replaced token");
    }

    [Fact]
    public async Task Should_return_validation_error_if_refresh_token_not_found()
    {
        // Arrange
        var client = _fixture.HttpClient.CreateClient();

        // Act
        var response = await client.GetAsync("/refreshToken");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsContract>();
        validationProblemDetails.Should().NotBeNull();

        var error = validationProblemDetails!.Errors.Single();
        error.Code.Should().Be("authenticate_validation_invalid_token");
    }

    [Fact]
    public async Task Should_return_validation_error_if_refresh_token_not_active()
    {
        // Arrange
        var passwordHasher = _scope.ServiceProvider.GetRequiredService<Argon2IdPasswordHasher>();

        var user = UserHelper.CreateUser("test@test.com", passwordHasher.HashPassword("qwerty123456A!"));

        await _fixture.Database.Execute(async x =>
        {
            x.Users.Add(user);
            await x.SaveChangesAsync();
        });

        var userToken = new UserToken
        {
            UserId = user.Id,
            Token = "test",
            Expires = DateTime.UtcNow.AddDays(-1).ToUniversalTime(),
            Created = DateTime.UtcNow.ToUniversalTime(),
        };

        await _fixture.Database.Execute(async x =>
        {
            x.UserTokens.Add(userToken);
            await x.SaveChangesAsync();
        });

        var client = _fixture.HttpClient.CreateClient();

        var message = new HttpRequestMessage(HttpMethod.Get, "/refreshToken");
        message.Headers.Add("Cookie", $"refresh-token={userToken.Token}");

        // Act
        var response = await client.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsContract>();
        validationProblemDetails.Should().NotBeNull();

        var error = validationProblemDetails!.Errors.Single();
        error.Code.Should().Be("authenticate_validation_invalid_token");
    }

    [Fact]
    public async Task Should_return_revoked_tokens_if_refresh_token_not_active()
    {
        // Arrange
        var passwordHasher = _scope.ServiceProvider.GetRequiredService<Argon2IdPasswordHasher>();

        var user = UserHelper.CreateUser("test@test.com", passwordHasher.HashPassword("qwerty123456A!"));

        await _fixture.Database.Execute(async x =>
        {
            x.Users.Add(user);
            await x.SaveChangesAsync();
        });

        var userToken1 = new UserToken
        {
            UserId = user.Id,
            Token = "test",
            Revoked = DateTime.UtcNow.ToUniversalTime(),
            Expires = DateTime.UtcNow.AddDays(-2).ToUniversalTime(),
            Created = DateTime.UtcNow.AddDays(-5).ToUniversalTime(),
        };

        var userToken2 = new UserToken
        {
            UserId = user.Id,
            Token = "test2",
            Expires = DateTime.UtcNow.AddDays(2).ToUniversalTime(),
            Created = DateTime.UtcNow.ToUniversalTime(),
        };

        await _fixture.Database.Execute(async x =>
        {
            x.UserTokens.AddRange(userToken1, userToken2);
            await x.SaveChangesAsync();
        });

        var client = _fixture.HttpClient.CreateClient();

        var message = new HttpRequestMessage(HttpMethod.Get, "/refreshToken");
        message.Headers.Add("Cookie", $"refresh-token={userToken1.Token}");

        // Act
        var response = await client.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsContract>();
        validationProblemDetails.Should().NotBeNull();

        var error = validationProblemDetails!.Errors.Single();
        error.Code.Should().Be("authenticate_validation_invalid_token");

        var userTokenContract1 = await _fixture.Database.Execute(async x =>
            await x.UserTokens.SingleAsync(x => x.Token == userToken1.Token));

        var userTokenContract2 = await _fixture.Database.Execute(async x =>
            await x.UserTokens.SingleAsync(x => x.Token == userToken2.Token));

        userTokenContract1.Revoked.Should().NotBeNull();
        userTokenContract1.ReasonRevoked.Should().Be("Invalid token");
        userTokenContract2.ReasonRevoked.Should().Be("Invalid token");
        userTokenContract2.Revoked.Should().NotBeNull();
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
