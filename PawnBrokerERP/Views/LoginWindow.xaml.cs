using Microsoft.Extensions.DependencyInjection;
using PawnBrokerERP.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PawnBrokerERP.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _vm;

    public LoginWindow(LoginViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;

        vm.LoginSucceeded += OnLoginSucceeded;
    }

    private void OnLoginSucceeded()
    {
        var mainWindow = App.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
        Close();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        => _vm.Password = PasswordBox.Password;

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        => Application.Current.Shutdown();
}
