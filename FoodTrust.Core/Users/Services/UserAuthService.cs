using System.Text.RegularExpressions;
using FoodTrust.Core.Admin.Interfaces;
using FoodTrust.Core.Users.Interfaces;
using FoodTrust.Core.Users.Models;

namespace FoodTrust.Core.Users.Services;

public sealed partial class UserAuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUserTokenGenerator tokenGenerator) : IUserAuthService
{
    /// <summary>
    /// 註冊一般會員並簽發存取權杖。
    /// </summary>
    public async Task<UserAuthResult> RegisterAsync(RegisterUserCommand command)
    {
        var email = NormalizeEmail(command.Email);
        var displayName = NormalizeDisplayName(command.DisplayName, email);
        ValidateRegistration(email, command.Password, displayName);

        if (await userRepository.FindByEmailAsync(email) is not null)
        {
            throw new InvalidOperationException("User email already exists.");
        }

        var user = await userRepository.CreateAsync(new CreateUserCommand(
            email,
            passwordHasher.Hash(command.Password),
            displayName,
            UserStatus.Active));

        var accessToken = tokenGenerator.Generate(user);
        return new UserAuthResult(accessToken.Token, accessToken.ExpiresAt, ToSummary(user));
    }

    /// <summary>
    /// 使用電子信箱與密碼登入一般會員。
    /// </summary>
    public async Task<UserAuthResult?> LoginAsync(LoginUserCommand command)
    {
        var email = NormalizeEmail(command.Email);
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

    /// <summary>
    /// 驗證註冊資料。
    /// </summary>
    private static void ValidateRegistration(string email, string password, string displayName)
    {
        if (!EmailRegex().IsMatch(email))
        {
            throw new ArgumentException("Invalid user email.", nameof(email));
        }

        if (password.Length < 12)
        {
            throw new ArgumentException("User password must be at least 12 characters.", nameof(password));
        }

        if (displayName.Length is < 2 or > 100)
        {
            throw new ArgumentException("User display name length is invalid.", nameof(displayName));
        }
    }

    /// <summary>
    /// 將電子信箱轉為標準格式。
    /// </summary>
    private static string NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email) ? string.Empty : email.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// 將顯示名稱轉為標準格式。
    /// </summary>
    private static string NormalizeDisplayName(string? displayName, string email)
    {
        return string.IsNullOrWhiteSpace(displayName) ? email.Split('@')[0] : displayName.Trim();
    }

    /// <summary>
    /// 將會員資料轉為對外回傳摘要。
    /// </summary>
    private static UserSummary ToSummary(User user)
    {
        return new UserSummary(
            user.Id,
            user.Email,
            user.DisplayName,
            user.Status,
            user.CreatedAt);
    }

    [GeneratedRegex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();
}
