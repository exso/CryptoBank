using CryptoBank.Features.Accounts.Requests;
using CryptoBank.Tests.Integration.Fixtures;
using CryptoBank.Tests.Integration.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
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
        var user = UserHelper.CreateUser($"{Guid.NewGuid()}@test.com", Guid.NewGuid().ToString());

        var (account1, account2) = AccountsHelper.CreateAccounts(user, "BTC", 100);

        await _fixture.Database.Execute(async x =>
        {
            x.Users.Add(user);
            await x.SaveChangesAsync();
        });

        var jwt = AuthenticateHelper.GetAccessToken(user, _scope);

        var client = _fixture.HttpClient.CreateClient();
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

        var accountContract1 = contract!.Accounts.Single(x => x.Number == account1.Number);
        accountContract1!.Number.Should().Be(account1.Number);
        accountContract1.Currency.Should().Be(account1.Currency);
        accountContract1.Amount.Should().Be(account1.Amount);
        accountContract1.DateOfOpening.Date.Should().Be(account1.DateOfOpening.Date);
        accountContract1.UserEmail.Should().Be(user.Email);

        var accountContract2 = contract!.Accounts.Single(x => x.Number == account2.Number);
        accountContract2!.Number.Should().Be(account2.Number);
        accountContract2.Currency.Should().Be(account2.Currency);
        accountContract2.Amount.Should().Be(account2.Amount);
        accountContract2.DateOfOpening.Date.Should().Be(account2.DateOfOpening.Date);
        accountContract2.UserEmail.Should().Be(user.Email);
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
