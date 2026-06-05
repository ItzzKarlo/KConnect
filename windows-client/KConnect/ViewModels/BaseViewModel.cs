// !/windows-client/kconnect/ViewModels/BaseViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace KConnect.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private string _successMessage = "";
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private bool _hasSuccess;

    protected void SetError(string msg)
    {
        ErrorMessage = msg;
        HasError = !string.IsNullOrEmpty(msg);
        SuccessMessage = "";
        HasSuccess = false;
    }

    protected void SetSuccess(string msg)
    {
        SuccessMessage = msg;
        HasSuccess = !string.IsNullOrEmpty(msg);
        ErrorMessage = "";
        HasError = false;
    }

    protected void ClearMessages()
    {
        ErrorMessage = "";
        SuccessMessage = "";
        HasError = false;
        HasSuccess = false;
    }
}