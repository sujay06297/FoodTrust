using FoodTrust.Infrastructure;
using FoodTrust.Infrastructure.Data;
using FoodTrust.Worker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddFoodTrustInfrastructure(builder.Configuration);
builder.Services.AddHostedService<RestaurantImportWorker>();

var host = builder.Build();

await host.Services.GetRequiredService<DatabaseInitializer>().InitializeAsync();
await host.RunAsync();
