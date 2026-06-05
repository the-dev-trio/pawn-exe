namespace PawnBrokerERP.Services;

public interface IAuthService
{
    Task<bool> ValidateLoginAsync(string phoneNumber, string password);
    Task<bool> InitializeShopAsync(string adminToken, string phoneNumber, string password, string shopName);
}
