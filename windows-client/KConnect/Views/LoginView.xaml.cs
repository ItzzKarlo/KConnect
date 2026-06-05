using System.Windows.Controls;
using KConnect.ViewModels;

namespace KConnect.Views;

public partial class LoginView : UserControl
{
    private readonly LoginViewModel _vm;

    public LoginView()
    {
        InitializeComponent();

        // Set the ViewModel if not already set
        if (DataContext is not LoginViewModel vm)
        {
            vm = new LoginViewModel();
            DataContext = vm;
        }
        _vm = vm;

        // Password binding
        PwdBox.PasswordChanged += (_, _) => _vm.Password = PwdBox.Password;

        // Event subscriptions
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