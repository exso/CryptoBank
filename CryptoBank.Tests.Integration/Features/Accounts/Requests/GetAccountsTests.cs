using CryptoBank.Features.Accounts.Domain;
using CryptoBank.Features.Accounts.Requests;
using CryptoBank.Tests.Integration.Fixtures;
using CryptoBank.Tests.Integration.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace CryptoBank.Tests.Integration.Features.Accounts.Requests;

[Collection(AccountsTestsCollection.Name)]
public class GetAccountsTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;

    private AsyncServiceScope _scope;

    public GetAccountsTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Should_get_accounts()
    {
        // Arrange
        var (client, user) = await _fixture.HttpClient.CreateAuthenticatedClient(Create.CancellationToken());

        var account = new Account
        {
            Number = "ACC1",
            Currency = "BTC",
            Amount = 100,
            DateOfOpening = DateTime.Now.ToUniversalTime()
        };

        user.UserAccounts.Add(account);

        await _fixture.Database.Execute(async x =>
        {
            x.Users.Add(user);
            await x.SaveChangesAsync();
        });

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
    public async Task Should_forbid_if_invalid_token()
    {
        // Arrange
        var (client, _) = await _fixture.HttpClient.CreateWronglyAuthenticatedClient(Create.CancellationToken());

        // Act
        var response = await client.GetAsync("/getAccounts");

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
