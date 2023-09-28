using CryptoBank.Common.Passwords;
using CryptoBank.Database;
using CryptoBank.Features.Management.Domain;

namespace CryptoBank.Tests.Integration.Helpers;

public class UserHelper
{
    private readonly Context _context;
    private readonly Argon2IdPasswordHasher _passwordHasher;

    public UserHelper(Context context, Argon2IdPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<User> CreateUser(string email, string password)
    {
        var user = new User
        {
            Email = email,
            Password = _passwordHasher.HashPassword(password),
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

        await _context.Users.AddAsync(user);

        return user;
    }
}
