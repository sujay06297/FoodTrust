using FoodTrust.Core.Users.Models;

namespace FoodTrust.Core.Users.Interfaces;

public interface IUserTokenGenerator
{
    /// <summary>
    /// 為指定會員產生存取權杖。
    /// </summary>
    UserAccessToken Generate(User user);
}
