using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PawnBrokerERP.Data;

namespace PawnBrokerERP.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    [ObservableProperty] private BaseViewModel? _currentView;
    [ObservableProperty] private string _shopName = "PawnBroker ERP";
    [ObservableProperty] private string _activeNav = "Dashboard";

    public MainViewModel(AppDbContext db)
    {
        var license = db.AppLicense.FirstOrDefault();
        if (!string.IsNullOrEmpty(license?.ShopName))
            ShopName = license.ShopName;

        NavigateTo("Dashboard");
    }

    [RelayCommand]
    private void NavigateTo(string page)
    {
        ActiveNav = page;
        var sp = App.Services;
        CurrentView = page switch
        {
            "Dashboard"     => sp.GetRequiredService<DashboardViewModel>(),
            "NewPledge"     => sp.GetRequiredService<NewPledgeViewModel>(),
            "RedeemPledge"  => sp.GetRequiredService<RedeemPledgeViewModel>(),
            "ManagePledges" => sp.GetRequiredService<ManagePledgesViewModel>(),
            "PartPayment"   => sp.GetRequiredService<PartPaymentViewModel>(),
            "Customers"     => sp.GetRequiredService<CustomersViewModel>(),
            "Settings"      => sp.GetRequiredService<SettingsViewModel>(),
            _               => sp.GetRequiredService<DashboardViewModel>()
        };
    }
}
