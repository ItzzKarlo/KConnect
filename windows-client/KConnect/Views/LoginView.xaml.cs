using System.Windows.Controls;
using KConnect.ViewModels;

namespace KConnect.Views;

public partial class LoginView : UserControl
{
    private readonly LoginViewModel _vm;

    public LoginView()
    {
        InitializeComponent();
        _vm = (LoginViewModel)DataContext;

        PasswordBox.PasswordChanged += (_, _) => _vm.Password = PasswordBox.Password;

        _vm.LoginSucceeded += () =>
        {
            MainWindow.Instance.NavigateTo(new ChatView());
        };

        _vm.NavigateToRegister += () =>
        {
            MainWindow.Instance.NavigateTo(new RegisterView());
        };
    }
}