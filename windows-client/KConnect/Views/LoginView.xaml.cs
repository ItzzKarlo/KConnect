using System.Windows.Controls;
using KConnect.ViewModels;

namespace KConnect.Views;

public partial class LoginView : UserControl
{
    private readonly LoginViewModel _vm;

    public LoginView()
    {
        InitializeComponent();

        if (DataContext is not LoginViewModel vm)
        {
            vm = new LoginViewModel();
            DataContext = vm;
        }
        _vm = vm;

        // Password binding
        PwdBox.PasswordChanged += (_, _) => _vm.Password = PwdBox.Password;

        // Safe event subscriptions
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