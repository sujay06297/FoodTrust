namespace FoodTrust.Core.Admin.Models;

public static class AdminRole
{
    public const string Admin = "Admin";

    public const string ReviewModerator = "ReviewModerator";

    /// <summary>
    /// 判斷指定後台角色是否受支援。
    /// </summary>
    public static bool IsValid(string? role)
    {
        return role is Admin or ReviewModerator;
    }
}
