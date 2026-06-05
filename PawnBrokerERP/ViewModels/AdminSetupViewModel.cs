using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PawnBrokerERP.Data;
using PawnBrokerERP.Models;
using PawnBrokerERP.Services;

namespace PawnBrokerERP.ViewModels;

public partial class AdminSetupViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly IUsbHardwareService _usb;
    private readonly AppDbContext _db;

    public event Action? SetupCompleted;

    [ObservableProperty] private string _adminToken = string.Empty;
    [ObservableProperty] private string _phoneNumber = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;
    [ObservableProperty] private string _shopName = string.Empty;
    [ObservableProperty] private string _detectedSerial = "Detecting USB...";

    public AdminSetupViewModel(IAuthService auth, IUsbHardwareService usb, AppDbContext db)
    {
        _auth = auth;
        _usb = usb;
        _db = db;
        DetectUsb();
    }

    private void DetectUsb()
    {
        var serial = _usb.GetCurrentUsbSerial();
        DetectedSerial = string.IsNullOrEmpty(serial)
            ? "No USB detected — running in DEV mode"
            : $"USB Detected: ...{serial[^Math.Min(8, serial.Length)..]}";
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        ClearMessages();

        if (string.IsNullOrWhiteSpace(AdminToken))
        { ErrorMessage = "Admin token is required."; return; }

        if (PhoneNumber.Length != 10 || !PhoneNumber.All(char.IsDigit))
        { ErrorMessage = "Enter a valid 10-digit client phone number."; return; }

        if (Password.Length < 6)
        { ErrorMessage = "Password must be at least 6 characters."; return; }

        if (Password != ConfirmPassword)
        { ErrorMessage = "Passwords do not match."; return; }

        if (string.IsNullOrWhiteSpace(ShopName))
        { ErrorMessage = "Shop name is required."; return; }

        IsBusy = true;
        try
        {
            var ok = await _auth.InitializeShopAsync(AdminToken, PhoneNumber, Password, ShopName);
            if (!ok)
            {
                ErrorMessage = "Invalid admin token. Contact PawnBrokerERP support.";
                return;
            }

            var usbSerial = _usb.GetCurrentUsbSerial() ?? "DEV-FALLBACK-00000";

            var license = _db.AppLicense.FirstOrDefault();
            if (license == null)
            {
                _db.AppLicense.Add(new AppLicense
                {
                    UsbSerialNumber = usbSerial,
                    IsInitialized = true,
                    ShopName = ShopName,
                    InitializedAt = DateTime.UtcNow
                });
            }
            else
            {
                license.UsbSerialNumber = usbSerial;
                license.IsInitialized = true;
                license.ShopName = ShopName;
            }

            await _db.SaveChangesAsync();
            SetupCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Setup failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
