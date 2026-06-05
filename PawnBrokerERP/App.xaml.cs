using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PawnBrokerERP.Data;
using PawnBrokerERP.Services;
using PawnBrokerERP.ViewModels;
using PawnBrokerERP.Views;
using System.Windows;

namespace PawnBrokerERP;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        SQLitePCL.Batteries_V2.Init();

        var services = new ServiceCollection();
        RegisterServices(services);
        Services = services.BuildServiceProvider();

        InitializeDatabase();
        Boot();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(ServiceLifetime.Transient);

        services.AddTransient<IUsbHardwareService, UsbHardwareService>();
        services.AddTransient<IAuthService, AuthService>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<AdminSetupViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<NewPledgeViewModel>();
        services.AddTransient<RedeemPledgeViewModel>();
        services.AddTransient<ManagePledgesViewModel>();
        services.AddTransient<PartPaymentViewModel>();
        services.AddTransient<CustomersViewModel>();
        services.AddTransient<SettingsViewModel>();

        services.AddTransient<LoginWindow>();
        services.AddTransient<AdminSetupWindow>();
        services.AddTransient<MainWindow>();
    }

    private static void InitializeDatabase()
    {
        using var db = Services.GetRequiredService<AppDbContext>();
        db.InitializeDatabase();
    }

    private static void Boot()
    {
        using var db = Services.GetRequiredService<AppDbContext>();
        var license = db.AppLicense.FirstOrDefault();

        if (license == null || !license.IsInitialized)
        {
            var setupWindow = Services.GetRequiredService<AdminSetupWindow>();
            setupWindow.Show();
        }
        else
        {
            var loginWindow = Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }
    }
}
