using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PawnBrokerERP.Data;
using PawnBrokerERP.Services;

namespace PawnBrokerERP.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly IUsbHardwareService _usb;
    private readonly AppDbContext _db;

    public event Action? LoginSucceeded;

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    public LoginViewModel(IAuthService auth, IUsbHardwareService usb, AppDbContext db)
    {
        _auth = auth;
        _usb = usb;
        _db = db;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ClearMessages();

        if (PhoneNumber.Length != 10 || !PhoneNumber.All(char.IsDigit))
        {
            ErrorMessage = "Enter a valid 10-digit phone number.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Password is required.";
            return;
        }

        IsBusy = true;
        try
        {
            // USB dongle check
            var license = await _db.AppLicense.FirstOrDefaultAsync();
            if (license != null && license.IsInitialized && !string.IsNullOrEmpty(license.UsbSerialNumber))
            {
                var currentSerial = _usb.GetCurrentUsbSerial();
                if (currentSerial == null || currentSerial != license.UsbSerialNumber)
                {
                    ErrorMessage = "Piracy Detected: Unauthorized Drive. Please use the original secure USB.";
                    return;
                }
            }

            var valid = await _auth.ValidateLoginAsync(PhoneNumber, Password);
            if (valid)
            {
                LoginSucceeded?.Invoke();
            }
            else
            {
                ErrorMessage = "Invalid phone number or password.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
