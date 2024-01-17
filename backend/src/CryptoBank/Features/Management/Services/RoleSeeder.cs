using CryptoBank.Database;
using CryptoBank.Features.Management.Domain;

namespace CryptoBank.Features.Management.Services;

public class RoleSeeder
{
    private readonly Context _context;

    public RoleSeeder(Context context)
    {
        _context = context;
    }

    [Obsolete("TODO роли загружать с файла")]
    public async Task Seed()
    {
        CancellationTokenSource cancellationTokenSource = new();

        CancellationToken cancellationToken = cancellationTokenSource.Token;

        if (!_context.Roles.Any())
        {
            var roles = new List<Role>()
                {
                    new Role()
                    {
                        Name = "User", Description = "Обычный пользователь"
                    },
                    new Role()
                    {
                        Name = "Analyst", Description = "Аналитик"
                    },
                    new Role()
                    {
                        Name = "Administrator", Description = "Администратор"
                    }
                };

            await _context.Roles.AddRangeAsync(roles, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
