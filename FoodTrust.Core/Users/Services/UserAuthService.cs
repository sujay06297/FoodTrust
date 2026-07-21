using FoodTrust.Core.Admin.Interfaces;
using FoodTrust.Core.Users.Domain.ValueObjects;
using FoodTrust.Core.Users.Interfaces;
using FoodTrust.Core.Users.Models;

namespace FoodTrust.Core.Users.Services;

public sealed class UserAuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUserTokenGenerator tokenGenerator) : IUserAuthService
{
    public async Task<UserAuthResult> RegisterAsync(RegisterUserCommand command)
    {
        var email = UserEmail.Create(command.Email);
        var password = AccountPassword.Create(command.Password, nameof(command.Password));
        var displayName = DisplayName.Create(command.DisplayName, email);

        if (await userRepository.FindByEmailAsync(email.Value) is not null)
        {
            throw new InvalidOperationException("User email already exists.");
        }

        var user = await userRepository.CreateAsync(new CreateUserCommand(
            email.Value,
            passwordHasher.Hash(password.Value),
            displayName.Value,
            UserStatus.Active));

        var accessToken = tokenGenerator.Generate(user);
        return new UserAuthResult(accessToken.Token, accessToken.ExpiresAt, ToSummary(user));
    }

    public async Task<UserAuthResult?> LoginAsync(LoginUserCommand command)
    {
        var email = UserEmail.NormalizeForLogin(command.Email);
        if (email.Length == 0 || string.IsNullOrWhiteSpace(command.Password))
        {
            return null;
        }

        var user = await userRepository.FindByEmailAsync(email);
        if (user is null ||
            user.Status != UserStatus.Active ||
            !passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            return null;
        }

        var accessToken = tokenGenerator.Generate(user);
        return new UserAuthResult(accessToken.Token, accessToken.ExpiresAt, ToSummary(user));
    }

    private static UserSummary ToSummary(User user)
    {
        return new UserSummary(
            user.Id,
            user.Email,
            user.DisplayName,
            user.Status,
            user.CreatedAt);
    }
}
