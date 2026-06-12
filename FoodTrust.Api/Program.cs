using FoodTrust.Api;
using FoodTrust.Api.Filters;
using FoodTrust.Api.Options;
using FoodTrust.Api.Security;
using FoodTrust.Core.Admin.Models;
using FoodTrust.Infrastructure;
using FoodTrust.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var adminJwtOptions = builder.Configuration
    .GetSection(AdminJwtOptions.SectionName)
    .Get<AdminJwtOptions>() ?? new AdminJwtOptions();
var userJwtOptions = builder.Configuration
    .GetSection(UserJwtOptions.SectionName)
    .Get<UserJwtOptions>() ?? new UserJwtOptions();
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:3000", "http://127.0.0.1:3000"];

builder.Services.AddFoodTrustApiServices();
builder.Services.AddFoodTrustInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = [adminJwtOptions.Issuer, userJwtOptions.Issuer],
            ValidateAudience = true,
            ValidAudiences = [adminJwtOptions.Audience, userJwtOptions.Audience],
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys =
            [
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(adminJwtOptions.SigningKey)),
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(userJwtOptions.SigningKey))
            ],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AdminPolicies.ReviewModeration,
        policy => policy.RequireRole(AdminRole.Admin, AdminRole.ReviewModerator));
});
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

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
