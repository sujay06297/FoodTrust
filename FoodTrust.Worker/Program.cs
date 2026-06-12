using FoodTrust.Infrastructure;
using FoodTrust.Infrastructure.Data;
using FoodTrust.Worker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

builder.Services.AddFoodTrustInfrastructure(builder.Configuration);
builder.Services.AddHostedService<RestaurantImportWorker>();

var host = builder.Build();

await host.Services.GetRequiredService<DatabaseInitializer>().InitializeAsync();
await host.RunAsync();
