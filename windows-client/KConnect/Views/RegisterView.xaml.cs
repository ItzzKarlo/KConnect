using System.Windows.Controls;
using KConnect.ViewModels;

namespace KConnect.Views;

public partial class RegisterView : UserControl
{
    private readonly RegisterViewModel _vm;

    public RegisterView()
    {
        InitializeComponent();
        _vm = (RegisterViewModel)DataContext;

        PasswordBox.PasswordChanged += (_, _) => _vm.Password = PasswordBox.Password;
        ConfirmPasswordBox.PasswordChanged += (_, _) => _vm.ConfirmPassword = ConfirmPasswordBox.Password;

        _vm.RegisterSucceeded += () =>
        {
            // Stay on register view, success message shown
            // User needs to verify email before they can log in
        };

        _vm.NavigateToLogin += () =>
        {
            MainWindow.Instance.NavigateTo(new LoginView());
        };
    }
}