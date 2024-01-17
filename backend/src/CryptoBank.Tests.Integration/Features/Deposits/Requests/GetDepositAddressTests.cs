using CryptoBank.Errors.Exceptions;
using CryptoBank.Features.Deposits.Requests;
using CryptoBank.Tests.Integration.Fixtures;
using CryptoBank.Tests.Integration.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CryptoBank.Tests.Integration.Features.Deposits.Requests;

[Collection(DepositsTestsCollection.Name)]
public class GetDepositAddressTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;

    private AsyncServiceScope _scope;

    public GetDepositAddressTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Should_get_deposits_address()
    {
        // Arrange
        var (client, _) = await _fixture.HttpClient.CreateAuthenticatedClient(Create.CancellationToken());

        await DepositsHelper.CreateCurrency(_fixture, Create.CancellationToken());
        await DepositsHelper.CreateVariable(_fixture, Create.CancellationToken());
        await DepositsHelper.CreateXpub(_fixture, Create.CancellationToken());

        // Act
        var response = (await client.PostAsync("/getDepositAddress", new StringContent(string.Empty)))
            .EnsureSuccessStatusCode();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Should().NotBeNull();

        var content = await response.Content.ReadAsStringAsync();
        var contract = JsonSerializer.Deserialize<GetDepositAddress.Response>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        contract!.CryptoAddress.Should().NotBeNull();

        var depositAddress = await _fixture.Database.Execute(async x =>
            await x.DepositAddresses.SingleOrDefaultAsync(u => u.CryptoAddress == contract.CryptoAddress));

        depositAddress.Should().NotBeNull();
        depositAddress!.CryptoAddress.Should().Be(contract.CryptoAddress);
    }

    [Fact]
    public async Task Should_forbid_if_invalid_token()
    {
        // Arrange
        var (client, _) = await _fixture.HttpClient.CreateWronglyAuthenticatedClient(Create.CancellationToken());

        // Act
        var response = await client.PostAsync("/getDepositAddress", new StringContent(string.Empty));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_return_logic_conflict_if_currency_not_exists()
    {
        // Arrange
        var (client, _) = await _fixture.HttpClient.CreateAuthenticatedClient(Create.CancellationToken());

        // Act
        var response = await client.PostAsync("/getDepositAddress", new StringContent(string.Empty));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var logicConflictException = await response.Content.ReadFromJsonAsync<LogicConflictException>();
        logicConflictException.Should().NotBeNull();
        logicConflictException!.Code.Should().Be("deposits_logic_conflict_currency_not_exist");
    }

    [Fact]
    public async Task Should_return_logic_conflict_if_xpub_not_exists()
    {
        // Arrange
        var (client, _) = await _fixture.HttpClient.CreateAuthenticatedClient(Create.CancellationToken());

        await DepositsHelper.CreateCurrency(_fixture, Create.CancellationToken());

        // Act
        var response = await client.PostAsync("/getDepositAddress", new StringContent(string.Empty));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var logicConflictException = await response.Content.ReadFromJsonAsync<LogicConflictException>();
        logicConflictException.Should().NotBeNull();
        logicConflictException!.Code.Should().Be("deposits_logic_conflict_xpub_not_exist");
    }

    [Fact]
    public async Task Should_return_logic_conflict_if_derivation_index_not_exists()
    {
        // Arrange
        var (client, _) = await _fixture.HttpClient.CreateAuthenticatedClient(Create.CancellationToken());

        await DepositsHelper.CreateCurrency(_fixture, Create.CancellationToken());
        await DepositsHelper.CreateXpub(_fixture, Create.CancellationToken());

        // Act
        var response = await client.PostAsync("/getDepositAddress", new StringContent(string.Empty));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var logicConflictException = await response.Content.ReadFromJsonAsync<LogicConflictException>();
        logicConflictException.Should().NotBeNull();
        logicConflictException!.Code.Should().Be("deposits_logic_conflict_derivation_index_not_exist");
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
