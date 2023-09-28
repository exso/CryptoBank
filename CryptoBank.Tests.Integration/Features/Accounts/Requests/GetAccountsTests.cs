using CryptoBank.Common.Passwords;
using CryptoBank.Database;
using CryptoBank.Features.Accounts.Domain;
using CryptoBank.Features.Accounts.Requests;
using CryptoBank.Tests.Integration.Common;
using CryptoBank.Tests.Integration.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CryptoBank.Tests.Integration.Features.Accounts.Requests;

public class GetAccountsTests : IClassFixture<BaseWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly BaseWebAppFactory<Program> _factory;
    private Context _context;
    private AsyncServiceScope _scope;
    private Argon2IdPasswordHasher _passwordHasher;

    public GetAccountsTests(BaseWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_get_accounts()
    {
        // Arrange
        var user = await new UserHelper(_context, _passwordHasher).CreateUser("me@example.com", "12345678");

        var account = new Account
        {
            Number = "ACC1",
            Currency = "BTC",
            Amount = 100,
            DateOfOpening = DateTime.Now.ToUniversalTime()
        };

        user.UserAccounts.Add(account);
        await _context.SaveChangesAsync();

        var jwt = AuthenticateHelper.GetAccessToken(user, _scope);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = (await client.GetAsync("/getAccounts"))
            .EnsureSuccessStatusCode();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Should().NotBeNull();

        var content = await response.Content.ReadAsStringAsync();
        var contract = JsonSerializer.Deserialize<GetAccounts.Response>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        var accountContract = contract!.Accounts.Single(x => x.Number == account.Number);
        accountContract.Number.Should().Be(account.Number);
        accountContract.Currency.Should().Be(account.Currency);
        accountContract.Amount.Should().Be(account.Amount);
        accountContract.DateOfOpening.Date.Should().Be(account.DateOfOpening.Date);
        accountContract.UserEmail.Should().Be(user.Email);
    }

    [Fact]
    public async Task Should_invalid_token()
    {
        // Arrange
        var jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwia";

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await client.GetAsync("/getAccounts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public Task InitializeAsync()
    {
        var _ = _factory.Server;
        _scope = _factory.Services.CreateAsyncScope();
        _context = _scope.ServiceProvider.GetRequiredService<Context>();
        _passwordHasher = _scope.ServiceProvider.GetRequiredService<Argon2IdPasswordHasher>();

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _context.Accounts.RemoveRange(_context.Accounts);
        _context.UserRoles.RemoveRange(_context.UserRoles);
        _context.Roles.RemoveRange(_context.Roles);
        _context.Users.RemoveRange(_context.Users);

        await _context.DisposeAsync();
        await _scope.DisposeAsync();
    }
}
