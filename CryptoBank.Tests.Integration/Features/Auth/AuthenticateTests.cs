using CryptoBank.Common.Passwords;
using CryptoBank.Features.Authenticate.Models;
using CryptoBank.Features.Authenticate.Options;
using CryptoBank.Features.Authenticate.Requests;
using CryptoBank.Tests.Integration.Errors.Contracts;
using CryptoBank.Tests.Integration.Fixtures;
using CryptoBank.Tests.Integration.Helpers;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

namespace CryptoBank.Tests.Integration.Features.Auth;

[Collection(AuthTestsCollection.Name)]
public class AuthenticateTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;

    private AsyncServiceScope _scope;

    public AuthenticateTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Should_authenticate_user()
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

        // Act
        var response = await client.PostAsJsonAsync<AuthenticateModel>("/authenticate", new
        {
            Email = "test@test.com",
            Password = "qwerty123456A!",
        });

        // Assert
        response.Should().NotBeNull();
        response.AccessToken.Should().NotBeNullOrEmpty();

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtOptions = _scope.ServiceProvider.GetRequiredService<IOptions<AuthenticateOptions>>().Value.Jwt;
        var key = jwtOptions.SigningKey;
        tokenHandler.ValidateToken(response.AccessToken, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ClockSkew = TimeSpan.Zero
        }, out var validatedToken);

        validatedToken.Should().NotBeNull();
        var jwtToken = (JwtSecurityToken)validatedToken;
        var userId = long.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);

        userId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Should_return_validation_error_if_user_not_found_by_email()
    {
        // Arrange

        // No user in DB

        var client = _fixture.HttpClient.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync<ValidationProblemDetailsContract>("/authenticate", new
        {
            Email = "test@test.com",
            Password = "qwerty123456A!",
        }, HttpStatusCode.BadRequest);

        // Assert
        response.ShouldContain("LowercaseEmail", "Invalid credentials", "authenticate_validation_invalid_credentials");
    }

    [Fact]
    public async Task Should_return_validation_error_if_password_is_incorrect()
    {
        // Arrange
        var passwordHasher = _scope.ServiceProvider.GetRequiredService<Argon2IdPasswordHasher>();

        var user = UserHelper.CreateUser("test@test.com", passwordHasher.HashPassword("12345678"));

        await _fixture.Database.Execute(async x =>
        {
            x.Users.Add(user);
            await x.SaveChangesAsync();
        });

        var client = _fixture.HttpClient.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync<ValidationProblemDetailsContract>("/authenticate", new
        {
            Email = "test@test.com",
            Password = "qwerty123456A!",
        }, HttpStatusCode.BadRequest);

        // Assert
        response.ShouldContain("LowercaseEmail", "Invalid credentials", "authenticate_validation_invalid_credentials");
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

[Collection(AuthTestsCollection.Name)]
public class AuthenticateValidatorTests 
{
    private readonly Authenticate.RequestValidator _validator = new();

    [Fact]
    public async Task Should_validate_correct_request()
    {
        var result = await _validator.TestValidateAsync(new Authenticate.Request("test@test.com", "password"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_require_email(string email)
    {
        var result = await _validator.TestValidateAsync(new Authenticate.Request(email, "password"));
        result.ShouldHaveValidationErrorFor(x => x.LowercaseEmail).WithErrorCode("authenticate_validation_email_required");
    }

    [Theory]
    [InlineData("test")]
    [InlineData("@test.com")]
    [InlineData("test@")]
    public async Task Should_validate_email_format(string email)
    {
        var result = await _validator.TestValidateAsync(new Authenticate.Request(email, "password"));
        result.ShouldHaveValidationErrorFor(x => x.LowercaseEmail).WithErrorCode("authenticate_validation_invalid_credentials");
    }

    [Theory]
    [InlineData("t")]
    [InlineData("ttt")]
    [InlineData("tttt")]
    public async Task Should_validate_email_minimum_length(string email)
    {
        var result = await _validator.TestValidateAsync(new Authenticate.Request(email, "123456"));
        result.ShouldHaveValidationErrorFor(x => x.LowercaseEmail).WithErrorCode("MinimumLengthValidator");
    }

    [Fact]
    public async Task Should_validate_email_maximum_length()
    {
        string email = new('A', 21);

        var result = await _validator.TestValidateAsync(new Authenticate.Request(email, "123456"));
        result.ShouldHaveValidationErrorFor(x => x.LowercaseEmail).WithErrorCode("MaximumLengthValidator");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_require_password(string password)
    {
        var result = await _validator.TestValidateAsync(new Authenticate.Request("test@test.com", password));
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorCode("authenticate_validation_password_required");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("123")]
    [InlineData("1234")]
    public async Task Should_validate_password_minimum_length(string password)
    {
        var result = await _validator.TestValidateAsync(new Authenticate.Request("test@test.com", password));
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorCode("MinimumLengthValidator");
    }

    [Fact]
    public async Task Should_validate_password_maximum_length()
    {
        string password = new('A', 21);

        var result = await _validator.TestValidateAsync(new Authenticate.Request("test@test.com", password));
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorCode("MaximumLengthValidator");
    }
}
