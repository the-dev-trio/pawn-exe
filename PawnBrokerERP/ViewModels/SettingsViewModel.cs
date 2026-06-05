using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PawnBrokerERP.Data;

namespace PawnBrokerERP.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly AppDbContext _db;

    [ObservableProperty] private string _shopName = string.Empty;
    [ObservableProperty] private string _usbSerial = string.Empty;
    [ObservableProperty] private string _appVersion = "v1.0.0";
    [ObservableProperty] private string _dbPath = string.Empty;

    public SettingsViewModel(AppDbContext db)
    {
        _db = db;
        LoadSettings();
    }

    private void LoadSettings()
    {
        var license = _db.AppLicense.FirstOrDefault();
        ShopName = license?.ShopName ?? "Not configured";
        UsbSerial = license?.UsbSerialNumber ?? "Not locked";
        DbPath = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                ?? AppDomain.CurrentDomain.BaseDirectory,
            "PawnERP.db");
    }

    [RelayCommand]
    private async Task SaveShopNameAsync()
    {
        ClearMessages();
        if (string.IsNullOrWhiteSpace(ShopName))
        { ErrorMessage = "Shop name cannot be empty."; return; }

        try
        {
            var license = _db.AppLicense.FirstOrDefault();
            if (license != null)
            {
                license.ShopName = ShopName;
                await _db.SaveChangesAsync();
                SuccessMessage = "Shop name saved.";
            }
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }
}
