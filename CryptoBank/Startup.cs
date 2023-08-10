using CryptoBank.Database;
using CryptoBank.Options;

using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CryptoBank
{
    public class Startup
    {
        private static readonly Assembly[] DefaultHandlerAssemblies = new[] {
                    typeof(Program).Assembly,
                    typeof(CryptoBank.Handlers.DbContextBase).Assembly
        };

        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // получаем строку подключения из файла конфигурации
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("DefaultConnection", "Please provide valid connection string");

            services.AddControllers();

            services.AddDbContext<DbContext, Context>(options => options.UseNpgsql(connectionString));

            services.AddMediatR(a => a.RegisterServicesFromAssemblies(DefaultHandlerAssemblies));

            services.Configure<NewsOptions>(_configuration.GetSection("LatestNews"));
        }

        public void Configure(WebApplication app)
        {
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            MigrateDatabase(app);
        }

        private static async void MigrateDatabase(WebApplication app)
        {
            await using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<Context>();
            await context.Database.MigrateAsync();
        }
    }
}