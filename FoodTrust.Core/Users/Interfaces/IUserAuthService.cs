using FoodTrust.Core.Users.Models;

namespace FoodTrust.Core.Users.Interfaces;

public interface IUserAuthService
{
    /// <summary>
    /// 註冊一般會員並簽發存取權杖。
    /// </summary>
    Task<UserAuthResult> RegisterAsync(RegisterUserCommand command);

    /// <summary>
    /// 使用電子信箱與密碼登入一般會員。
    /// </summary>
    Task<UserAuthResult?> LoginAsync(LoginUserCommand command);
}
