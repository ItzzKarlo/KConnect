using System.Windows.Controls;
using KConnect.ViewModels;

namespace KConnect.Views;

public partial class RegisterView : UserControl
{
    private readonly RegisterViewModel _vm;

    public RegisterView()
    {
        InitializeComponent();

        // DataContext is set in XAML — just grab it
        _vm = (RegisterViewModel)DataContext;

        // Wire PasswordBoxes manually (WPF security — Password can't be bound in XAML)
        // Note: named PwdBox / ConfirmPwdBox to avoid collision with the PasswordBox class name
        PwdBox.PasswordChanged        += (_, _) => _vm.Password        = PwdBox.Password;
        ConfirmPwdBox.PasswordChanged += (_, _) => _vm.ConfirmPassword = ConfirmPwdBox.Password;

        // Navigation events
        _vm.NavigateToLogin += () =>
        {
            MainWindow.Instance?.NavigateTo(new LoginView());
        };
    }
}
