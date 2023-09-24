using CryptoBank.Common.Passwords;
using CryptoBank.Database;
using CryptoBank.Features.Accounts.Requests;
using CryptoBank.Features.Management.Domain;
using CryptoBank.Tests.Integration.Common;
using CryptoBank.Tests.Integration.Errors.Contracts;
using CryptoBank.Tests.Integration.Helpers;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CryptoBank.Tests.Integration.Features.Accounts.Requests;

public class CreateAccountTests : IClassFixture<BaseWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly BaseWebAppFactory<Program> _factory;
    private Context _context;
    private AsyncServiceScope _scope;
    private Argon2IdPasswordHasher _passwordHasher;

    public CreateAccountTests(BaseWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_create_account()
    {
        // Arrange
        var user = await new UserHelper(_context, _passwordHasher).CreateUser("me@example.com", "12345678");

        var jwt = AuthenticateHelper.GetAccessToken(user, _scope);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = (await client.PostAsJsonAsync("/createAccount", new
        {
            Currency = "BTC",
            Amount = 100,
            UserId = user.Id
        })).EnsureSuccessStatusCode();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var account = await _context.Accounts.SingleOrDefaultAsync(x => x.UserId == user.Id);
        account.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_invalid_token()
    {
        // Arrange
        var user = await new UserHelper(_context, _passwordHasher).CreateUser("me@example.com", "12345678");

        var jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwia";

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await client.PostAsJsonAsync("/createAccount", new
        {
            Currency = "BTC",
            Amount = 100,
            UserId = user.Id
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_user_not_found()
    {
        // Arrange
        var user = new User
        {
            Email = "me@example.com",
            Password = _passwordHasher.HashPassword("12345678"),
            DateOfBirth = new DateTime(2000, 01, 31).ToUniversalTime(),
            DateOfRegistration = DateTime.UtcNow,
            UserRoles = new List<UserRole>()
            {
                new()
                {
                    Role = new Role
                    {
                        Name = "User", Description = "Обычный пользователь"
                    }
                }
            }
        };

        var jwt = AuthenticateHelper.GetAccessToken(user, _scope);

        var client = _factory.CreateClient();
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
