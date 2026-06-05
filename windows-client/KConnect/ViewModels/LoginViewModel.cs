// !/windows-client/kconnect/ViewModels/LoginViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KConnect.Services;

namespace KConnect.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _auth = new();

    [ObservableProperty] private string _identifier = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private string _mfaCode = "";
    [ObservableProperty] private bool _mfaRequired;

    public event Action? LoginSucceeded;
    public event Action? NavigateToRegister;

    [RelayCommand]
    private async Task LoginAsync()
    {
        ClearMessages();
        if (string.IsNullOrWhiteSpace(Identifier) || string.IsNullOrWhiteSpace(Password))
        {
            SetError("Please enter your username/email and password");
            return;
        }

        IsBusy = true;
        var (success, message) = await _auth.LoginAsync(
            Identifier, Password, MfaRequired ? MfaCode : null);
        IsBusy = false;

        if (message == "MFA code required")
        {
            MfaRequired = true;
            SetError("Enter the 6-digit code from your authenticator app");
            return;
        }

        if (success) LoginSucceeded?.Invoke();
        else SetError(message);
    }
    
    [RelayCommand]
    private async Task ForgotPasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(Identifier) || !Identifier.Contains('@'))
        {
            SetError("Enter your email address in the field above first");
            return;
        }
        IsBusy = true;
        await _auth.ForgotPasswordAsync(Identifier);
        IsBusy = false;
        SetSuccess("If that email is registered, a reset link has been sent");
    }
    
    [RelayCommand]
    private void GoToRegister() => NavigateToRegister?.Invoke();
}