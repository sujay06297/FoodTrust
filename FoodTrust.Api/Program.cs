using FoodTrust.Api;
using FoodTrust.Api.Filters;
using FoodTrust.Infrastructure;
using FoodTrust.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFoodTrustApiServices();
builder.Services.AddFoodTrustInfrastructure(builder.Configuration);
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ArgumentExceptionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await app.Services.GetRequiredService<DatabaseInitializer>().InitializeAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
