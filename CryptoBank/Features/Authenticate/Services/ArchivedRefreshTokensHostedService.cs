using CryptoBank.Features.Authenticate.Options;
using Microsoft.Extensions.Options;

namespace CryptoBank.Features.Authenticate.Services;

public class ArchivedRefreshTokensHostedService : BackgroundService
{
    private readonly ILogger<ArchivedRefreshTokensHostedService> _logger;
    private readonly AuthenticateOptions _options;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ArchivedRefreshTokensHostedService(
        ILogger<ArchivedRefreshTokensHostedService> logger,
        IOptions<AuthenticateOptions> options,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _options = options.Value;
        _serviceScopeFactory = serviceScopeFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();

                var archiveRefreshTokensProcessor = scope.ServiceProvider.GetRequiredService<ITokenService>();

                await archiveRefreshTokensProcessor.RemoveArchivedRefreshTokens(stoppingToken);

                _logger.LogInformation("Archived refresh tokens removed");

                await Task.Delay(_options.Jwt.IntervalRemovedArchivedRefreshTokens, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request execution error");
            }       
        } 
    }
}
