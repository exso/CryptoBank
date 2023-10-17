using CryptoBank.Features.Management.Domain;

namespace CryptoBank.Tests.Integration.Helpers;

public class UserHelper
{
    public static User CreateUser(string email, string password, bool isAnalyst = false)
    {
        var user = new User
        {
            Email = email,
            Password = password,
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

        if (isAnalyst)
        {
            user.UserRoles.Add(new() { Role = new Role { Name = "Analyst", Description = "Аналитик" } });
        }

        return user;
    }
}
