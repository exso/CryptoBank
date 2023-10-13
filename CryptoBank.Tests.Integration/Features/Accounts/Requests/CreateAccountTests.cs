using CryptoBank.Common.Passwords;
using CryptoBank.Features.Accounts.Requests;
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

namespace CryptoBank.Tests.Integration.Features.Accounts.Requests;

[Collection(AccountsTestsCollection.Name)]
public class CreateAccountTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;

    private AsyncServiceScope _scope;

    public CreateAccountTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Should_create_account()
    {
        // Arrange
        var (client, user) = await _fixture.HttpClient.CreateAuthenticatedClient(Create.CancellationToken());

        var currency = "BTC";
        var amount = 100;

        // Act
        var response = (await client.PostAsJsonAsync("/createAccount", new
        {
            Currency = currency,
            Amount = amount,
            UserId = user.Id
        })).EnsureSuccessStatusCode();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var account = await _fixture.Database.Execute(async x =>
            await x.Accounts.SingleOrDefaultAsync(u => u.UserId == user.Id));

        account.Should().NotBeNull();
        account!.Currency.Should().Be(currency);
        account.Amount.Should().Be(amount);
        account.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Should_forbid_if_invalid_token()
    {
        // Arrange
        var (client, _) = await _fixture.HttpClient.CreateWronglyAuthenticatedClient(Create.CancellationToken());

        // Act
        var response = await client.PostAsJsonAsync("/createAccount", new
        {
            Currency = "BTC",
            Amount = 100,
            UserId = 1
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_return_bad_request_if_user_not_found()
    {
        // Arrange
        var passwordHasher = _scope.ServiceProvider.GetRequiredService<Argon2IdPasswordHasher>();

        var user = UserHelper.CreateUser("me@example.com", passwordHasher.HashPassword("12345678"));

        var jwt = AuthenticateHelper.GetAccessToken(user, _scope);

        var client = _fixture.HttpClient.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await client.PostAsJsonAsync("/createAccount", new
        {
            Currency = "BTC",
            Amount = 100,
            UserId = user.Id
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsContract>();
        validationProblemDetails.Should().NotBeNull();

        var error = validationProblemDetails!.Errors.Single();
        error.Code.Should().Be("accounts_validation_user_not_found");
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

public class CreateAccountValidatorTests
{
    private readonly CreateAccount.RequestValidator _validator = new();

    [Fact]
    public void Should_validate_correct_request()
    {
        var result = _validator.TestValidate(new CreateAccount.Request("BTC", 100, 1));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_require_currency(string currency)
    {
        var result = await _validator.TestValidateAsync(new CreateAccount.Request(currency, 100, 1));
        result.ShouldHaveValidationErrorFor(x => x.Currency).WithErrorCode("NotEmptyValidator");
    }

    [Theory]
    [InlineData("q")]
    [InlineData("qq")]
    public async Task Should_validate_currency_minimum_length(string currency)
    {
        var result = await _validator.TestValidateAsync(new CreateAccount.Request(currency, 100, 1));
        result.ShouldHaveValidationErrorFor(x => x.Currency).WithErrorCode("MinimumLengthValidator");
    }

    [Fact]
    public async Task Should_validate_currency_maximum_length()
    {
        var result = await _validator.TestValidateAsync(new CreateAccount.Request("BTCC", 100, 1));
        result.ShouldHaveValidationErrorFor(x => x.Currency).WithErrorCode("MaximumLengthValidator");
    }

    [Fact]
    public async Task Should_require_amount()
    {
        var result = await _validator.TestValidateAsync(new CreateAccount.Request("BTC", 0, 1));
        result.ShouldHaveValidationErrorFor(x => x.Amount).WithErrorCode("NotEmptyValidator");
    }
}
