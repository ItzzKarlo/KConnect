// !/windows-client/kconnect/ViewModels/RegisterViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KConnect.Services;

namespace KConnect.ViewModels;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly AuthService _auth = new();

    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string _email = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private string _confirmPassword = "";
    [ObservableProperty] private string _phone = "";

    public event Action? RegisterSucceeded;
    public event Action? NavigateToLogin;

    [RelayCommand]
    private async Task RegisterAsync()
    {
        ClearMessages();
        if (Password != ConfirmPassword) { SetError("Passwords do not match"); return; }
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email))
        { SetError("Username and email are required"); return; }

        IsBusy = true;
        var (success, message) = await _auth.RegisterAsync(
            Username, Email, Password, string.IsNullOrWhiteSpace(Phone) ? null : Phone);
        IsBusy = false;

        if (success) { SetSuccess(message); RegisterSucceeded?.Invoke(); }
        else SetError(message);
    }

    [RelayCommand]
    private void GoToLogin() => NavigateToLogin?.Invoke();
}