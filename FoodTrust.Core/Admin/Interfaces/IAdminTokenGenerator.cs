using FoodTrust.Core.Admin.Models;

namespace FoodTrust.Core.Admin.Interfaces;

public interface IAdminTokenGenerator
{
    /// <summary>
    /// 為指定管理員產生後台存取權杖。
    /// </summary>
    AdminAccessToken Generate(AdminUser user);
}
