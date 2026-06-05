using Microsoft.EntityFrameworkCore;
using PawnBrokerERP.Data;
using PawnBrokerERP.Helpers;
using PawnBrokerERP.Models;

namespace PawnBrokerERP.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private const string AdminToken = "ADMIN-778899";

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ValidateLoginAsync(string phoneNumber, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (user == null) return false;
        return PasswordHelper.Verify(password, user.PasswordHash);
    }

    public async Task<bool> InitializeShopAsync(string adminToken, string phoneNumber, string password, string shopName)
    {
        if (adminToken != AdminToken) return false;
        if (phoneNumber.Length != 10 || !phoneNumber.All(char.IsDigit)) return false;

        var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (existingUser == null)
        {
            _db.Users.Add(new User
            {
                PhoneNumber = phoneNumber,
                PasswordHash = PasswordHelper.Hash(password)
            });
        }
        else
        {
            existingUser.PasswordHash = PasswordHelper.Hash(password);
            existingUser.LastUpdated = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return true;
    }
}
