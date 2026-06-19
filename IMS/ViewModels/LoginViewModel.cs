using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IMS.Models;
using IMS.Services;

namespace IMS.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isError;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter username and password.";
            IsError = true;
            return;
        }

        var success = _authService.Login(Username, Password);
        if (success)
        {
            IsError = false;
            // Navigate to the main app with persistent sidebar
            await Shell.Current.GoToAsync("//Main");
        }
        else
        {
            ErrorMessage = "Invalid credentials. Try: admin/admin, service/service, logistics/logistics";
            IsError = true;
        }
    }
}
