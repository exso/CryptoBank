using CryptoBank.Common.Passwords;
using CryptoBank.Database;
using CryptoBank.Tests.Integration.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;
using CryptoBank.Features.Accounts.Requests;
using FluentValidation.TestHelper;
using CryptoBank.Tests.Integration.Helpers;
using CryptoBank.Features.Accounts.Domain;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Tests.Integration.Features.Accounts.Requests;

public class TransferCashTests : IClassFixture<BaseWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly BaseWebAppFactory<Program> _factory;
    private Context _context;
    private AsyncServiceScope _scope;
    private Argon2IdPasswordHasher _passwordHasher;

    public TransferCashTests(BaseWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_transfer_cash()
    {
        // Arrange
        var user = await new UserHelper(_context, _passwordHasher).CreateUser("me@example.com", "12345678");

        var currency = "BTC";
        var amount = Decimal.Add(100, 100);

        var account1 = new Account
        {
            Number = "541e377c-9a38-46a5-a02c-0720dcc4cc2e",
            Currency = currency,
            Amount = 1000,
            DateOfOpening = DateTime.Now.ToUniversalTime()
        };

        var account2 = new Account
        {
            Number = "a53a4971-2ac7-4f89-9734-84dee9fa1d92",
            Currency = currency,
            Amount = 100,
            DateOfOpening = DateTime.Now.ToUniversalTime()
        };

        user.UserAccounts.Add(account1);
        user.UserAccounts.Add(account2);
        await _context.SaveChangesAsync();

        var jwt = AuthenticateHelper.GetAccessToken(user, _scope);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = (await client.PostAsJsonAsync("/transferCash", new
        {
            FromNumber = account1.Number,
            ToNumber = account2.Number,
            Amount = 100
        })).EnsureSuccessStatusCode();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var account = await _context.Accounts.SingleOrDefaultAsync(x => x.Number == account2.Number);
        account.Should().NotBeNull();
        account!.Number.Should().Be(account2.Number);
        account.Currency.Should().Be(currency);
        account.Amount.Should().Be(amount);
        account.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Should_invalid_token()
    {
        // Arrange
        var jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwia";

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

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

        await _context.SaveChangesAsync();
        await _context.DisposeAsync();
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
