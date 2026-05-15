using FoodTrust.Core.Users.Models;

namespace FoodTrust.Core.Users.Interfaces;

public interface IUserRepository
{
    /// <summary>
    /// 依電子信箱查詢會員。
    /// </summary>
    Task<User?> FindByEmailAsync(string email);

    /// <summary>
    /// 建立新的會員。
    /// </summary>
    Task<User> CreateAsync(CreateUserCommand command);
}
