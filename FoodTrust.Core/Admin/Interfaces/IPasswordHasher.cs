namespace FoodTrust.Core.Admin.Interfaces;

public interface IPasswordHasher
{
    /// <summary>
    /// 將明文密碼雜湊為可儲存格式。
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// 驗證明文密碼是否符合已儲存的雜湊值。
    /// </summary>
    bool Verify(string password, string passwordHash);
}
