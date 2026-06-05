using System.Windows.Controls;
using KConnect.ViewModels;

namespace KConnect.Views;

public partial class RegisterView : UserControl
{
    private readonly RegisterViewModel _vm;

    public RegisterView()
    {
        InitializeComponent();

        if (DataContext is not RegisterViewModel vm)
        {
            vm = new RegisterViewModel();
            DataContext = vm;
        }
        _vm = vm;

        // Password bindings
        PasswordBox.PasswordChanged += (_, _) => _vm.Password = PasswordBox.Password;
        ConfirmPasswordBox.PasswordChanged += (_, _) => _vm.ConfirmPassword = ConfirmPasswordBox.Password;

        // Safe navigation
        _vm.NavigateToLogin += () =>
        {
            MainWindow.Instance?.NavigateTo(new LoginView());
        };
    }
}