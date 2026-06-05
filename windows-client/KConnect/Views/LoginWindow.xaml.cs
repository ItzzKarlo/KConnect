using System.Windows;
using KConnect.ViewModels;

namespace KConnect.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _vm;

    public LoginWindow()
    {
        InitializeComponent();
        _vm = (LoginViewModel)DataContext;

        // Pass PasswordBox value manually (WPF security — can't bind PasswordBox.Password)
        PasswordBox.PasswordChanged += (_, _) => _vm.Password = PasswordBox.Password;

        _vm.LoginSucceeded += () =>
        {
            new ChatWindow().Show();
            Close();
        };

        _vm.NavigateToRegister += () =>
        {
            new RegisterWindow().Show();
            Close();
        };
        // Register converters
        Resources["BoolToVisibility"] = new Converters.BoolToVisibilityConverter();
        Resources["StringToVisibility"] = new Converters.StringToVisibilityConverter();
        Resources["InverseBool"] = new Converters.InverseBoolConverter();
        Resources["BusyToText"] = new Converters.BusyToTextConverter();
    }
}