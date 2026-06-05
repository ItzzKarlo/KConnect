// !/windows-client/kconnect/ViewModels/BaseViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace KConnect.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private string _successMessage = "";

    protected void SetError(string msg)
    {
        ErrorMessage = msg;
        SuccessMessage = "";
    }

    protected void SetSuccess(string msg)
    {
        ErrorMessage = "";
        SuccessMessage = msg;
    }

    protected void ClearMessages()
    {
        ErrorMessage = "";
        SuccessMessage = "";
    }
}