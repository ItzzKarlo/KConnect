using System.Windows.Controls;
using KConnect.ViewModels;

namespace KConnect.Views;

public partial class LoginView : UserControl
{
    private readonly LoginViewModel _vm;

    public LoginView()
    {
        InitializeComponent();

        // DataContext is set in XAML — just grab it
        _vm = (LoginViewModel)DataContext;

        // Wire PasswordBox manually (WPF security — Password can't be bound in XAML)
        PwdBox.PasswordChanged += (_, _) => _vm.Password = PwdBox.Password;

        // Navigation events
        _vm.LoginSucceeded += () =>
        {
            MainWindow.Instance?.NavigateTo(new ChatView());
        };

        _vm.NavigateToRegister += () =>
        {
            MainWindow.Instance?.NavigateTo(new RegisterView());
        };
    }
}
