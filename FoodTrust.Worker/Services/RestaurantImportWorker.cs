using FoodTrust.Core.RestaurantImports.Interfaces;
using FoodTrust.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodTrust.Worker.Services;

public sealed class RestaurantImportWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<RestaurantImportOptions> options,
    ILogger<RestaurantImportWorker> logger) : BackgroundService
{
    /// <summary>
    /// 執行匯入排程直到主機停止。
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (options.Value.RunOnStartup)
        {
            await RunImportAsync(stoppingToken);
        }

        var interval = TimeSpan.FromHours(Math.Max(1, options.Value.IntervalHours));
        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunImportAsync(stoppingToken);
        }
    }

    /// <summary>
    /// 在 scoped service provider 內執行一次餐廳匯入流程。
    /// </summary>
    private async Task RunImportAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var importService = scope.ServiceProvider.GetRequiredService<IRestaurantImportService>();

        try
        {
            logger.LogInformation("Restaurant import started.");
            await importService.ImportAsync(options.Value.BatchSize, cancellationToken);
            logger.LogInformation("Restaurant import succeeded.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Restaurant import failed.");
        }
    }
}
