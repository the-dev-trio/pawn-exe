using Microsoft.Extensions.DependencyInjection;
using PawnBrokerERP.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PawnBrokerERP.Views;

public partial class AdminSetupWindow : Window
{
    private readonly AdminSetupViewModel _vm;

    public AdminSetupWindow(AdminSetupViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
        vm.SetupCompleted += OnSetupCompleted;
    }

    private void OnSetupCompleted()
    {
        var mainWindow = App.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
        Close();
    }

    private void AdminTokenBox_PasswordChanged(object sender, RoutedEventArgs e)
        => _vm.AdminToken = AdminTokenBox.Password;

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        => _vm.Password = PasswordBox.Password;

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        => _vm.ConfirmPassword = ConfirmPasswordBox.Password;

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }
}
