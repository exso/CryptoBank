using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;
using CryptoBank.Features.Accounts.Requests;
using FluentValidation.TestHelper;
using CryptoBank.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;
using CryptoBank.Errors.Exceptions;
using CryptoBank.Tests.Integration.Fixtures;

namespace CryptoBank.Tests.Integration.Features.Accounts.Requests;

[Collection(AccountsTestsCollection.Name)]
public class TransferCashTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;

    private AsyncServiceScope _scope;

    public TransferCashTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Should_transfer_cash()
    {
        // Arrange
        var (client, user) = await _fixture.HttpClient.CreateAuthenticatedClient(Create.CancellationToken());

        var currency = "BTC";
        var amount = Decimal.Add(100, 100);
        decimal fromAmount = 1000;
        decimal toAmount = 100;

        var (currentUser, account1, account2) = AccountsHelper.CreateAccounts(user, currency, fromAmount, toAmount);

        await _fixture.Database.Execute(async x =>
        {
            x.Users.Add(currentUser);
            await x.SaveChangesAsync();
        });

        // Act
        var response = (await client.PostAsJsonAsync("/transferCash", new
        {
            FromNumber = account1.Number,
            ToNumber = account2.Number,
            Amount = 100
        })).EnsureSuccessStatusCode();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var accountContract1 = await _fixture.Database.Execute(async x =>
            await x.Accounts.SingleOrDefaultAsync(u => u.Number == account1.Number));

        AccountsHelper.AssertAccount(accountContract1!, account1);

        var accountContract2 = await _fixture.Database.Execute(async x =>
            await x.Accounts.SingleOrDefaultAsync(u => u.Number == account2.Number));

        AccountsHelper.AssertAccount(accountContract2!, account2);
    }

    [Fact]
    public async Task Should_insufficient_amount_in_the_account()
    {
        // Arrange
        var (client, user) = await _fixture.HttpClient.CreateAuthenticatedClient(Create.CancellationToken());

        var currency = "BTC";
        decimal fromAmount = 100;
        decimal toAmount = 100;

        var (currentUser, account1, account2) = AccountsHelper.CreateAccounts(user, currency, fromAmount, toAmount);

        await _fixture.Database.Execute(async x =>
        {
            x.Users.Add(currentUser);
            await x.SaveChangesAsync();
        });

        // Act
        var response = await client.PostAsJsonAsync("/transferCash", new
        {
            FromNumber = account1.Number,
            ToNumber = account2.Number,
            Amount = 200
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var logicConflictException = await response.Content.ReadFromJsonAsync<LogicConflictException>();
        logicConflictException.Should().NotBeNull();
        logicConflictException!.Code.Should().Be("accounts_logic_confict_insufficient_amount_in_the_account");
    }

    [Fact]
    public async Task Should_forbid_if_invalid_token()
    {
        // Arrange
        var (client, _) = await _fixture.HttpClient.CreateWronglyAuthenticatedClient(Create.CancellationToken());

        // Act
        var response = await client.PostAsJsonAsync("/transferCash", new
        {
            FromNumber = "ACC1",
            ToNumber = "ACC2",
            Amount = 100
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

public class TransferCashValidatorTests
{
    private readonly TransferCash.RequestValidator _validator = new();

    [Fact]
    public void Should_validate_correct_request()
    {
        var fromNumber = "541e377c-9a38-46a5-a02c-0720dcc4cc2e";
        var toNumber = "a53a4971-2ac7-4f89-9734-84dee9fa1d92";

        var result = _validator.TestValidate(new TransferCash.Request(fromNumber, toNumber, 100));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_require_fromNumber(string fromNumber)
    {
        var toNumber = "a53a4971-2ac7-4f89-9734-84dee9fa1d92";

        var result = await _validator.TestValidateAsync(new TransferCash.Request(fromNumber, toNumber, 100));
        result.ShouldHaveValidationErrorFor(x => x.FromNumber).WithErrorCode("NotEmptyValidator");
    }

    [Theory]
    [InlineData("q")]
    [InlineData("qq")]
    [InlineData("qqq")]
    public async Task Should_validate_fromNumber_minimum_length(string fromNumber)
    {
        var toNumber = "a53a4971-2ac7-4f89-9734-84dee9fa1d92";

        var result = await _validator.TestValidateAsync(new TransferCash.Request(fromNumber, toNumber, 100));
        result.ShouldHaveValidationErrorFor(x => x.FromNumber).WithErrorCode("MinimumLengthValidator");
    }

    [Fact]
    public async Task Should_validate_fromNumber_maximum_length()
    {
        string fromNumber = new('A', 257);
        var toNumber = "a53a4971-2ac7-4f89-9734-84dee9fa1d92";

        var result = await _validator.TestValidateAsync(new TransferCash.Request(fromNumber, toNumber, 100));
        result.ShouldHaveValidationErrorFor(x => x.FromNumber).WithErrorCode("MaximumLengthValidator");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_require_toNumber(string toNumber)
    {
        var fromNumber = "a53a4971-2ac7-4f89-9734-84dee9fa1d92";

        var result = await _validator.TestValidateAsync(new TransferCash.Request(fromNumber, toNumber, 100));
        result.ShouldHaveValidationErrorFor(x => x.ToNumber).WithErrorCode("NotEmptyValidator");
    }

    [Theory]
    [InlineData("q")]
    [InlineData("qq")]
    [InlineData("qqq")]
    public async Task Should_validate_toNumber_minimum_length(string toNumber)
    {
        var fromNumber = "a53a4971-2ac7-4f89-9734-84dee9fa1d92";

        var result = await _validator.TestValidateAsync(new TransferCash.Request(fromNumber, toNumber, 100));
        result.ShouldHaveValidationErrorFor(x => x.ToNumber).WithErrorCode("MinimumLengthValidator");
    }

    [Fact]
    public async Task Should_validate_toNumber_maximum_length()
    {
        var fromNumber = "a53a4971-2ac7-4f89-9734-84dee9fa1d92";
        string toNumber = new('A', 257);

        var result = await _validator.TestValidateAsync(new TransferCash.Request(fromNumber, toNumber, 100));
        result.ShouldHaveValidationErrorFor(x => x.ToNumber).WithErrorCode("MaximumLengthValidator");
    }

    [Fact]
    public async Task Should_require_amount()
    {
        var fromNumber = "541e377c-9a38-46a5-a02c-0720dcc4cc2e";
        var toNumber = "a53a4971-2ac7-4f89-9734-84dee9fa1d92";

        var result = await _validator.TestValidateAsync(new TransferCash.Request(fromNumber, toNumber, 0));
        result.ShouldHaveValidationErrorFor(x => x.Amount).WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public async Task Should_validate_amount_greater_than()
    {
        var fromNumber = "541e377c-9a38-46a5-a02c-0720dcc4cc2e";
        var toNumber = "a53a4971-2ac7-4f89-9734-84dee9fa1d92";

        var result = await _validator.TestValidateAsync(new TransferCash.Request(fromNumber, toNumber, 0));
        result.ShouldHaveValidationErrorFor(x => x.Amount).WithErrorCode("GreaterThanValidator");
    }
}
