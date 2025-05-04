using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using EHRp.Messages;
using EHRp.Models;
using EHRp.Services;
using Microsoft.Extensions.Logging;

namespace EHRp.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService? _authService;
        private readonly ILogger<LoginViewModel>? _logger;
        private CancellationTokenSource? _cts;
        
        [ObservableProperty]
        private string _username = "";
        
        [ObservableProperty]
        private string _password = "";
        
        [ObservableProperty]
        private string _errorMessage = "";
        
        [ObservableProperty]
        private bool _isLoading;
        
        // Default constructor for design-time support
        public LoginViewModel()
        {
        }
        
        [RelayCommand]
        private void ForgotPassword()
        {
            // Display a message to the user
            ErrorMessage = "Please contact your system administrator to reset your password.";
            
            // Log the action
            _logger?.LogInformation("User requested password reset");
        }
        
        public LoginViewModel(IAuthService authService, ILogger<LoginViewModel> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cts = new CancellationTokenSource();
        }
        
        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Username and password are required";
                return;
            }
            
            try
            {
                IsLoading = true;
                ErrorMessage = "";
                
                // Cancel any previous login attempt
                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                
                if (_authService == null)
                {
                    ErrorMessage = "Authentication service is not available";
                    return;
                }
                
                var user = await _authService.AuthenticateAsync(Username, Password, _cts.Token);
                
                if (user != null)
                {
                    // Login successful - the message is sent by the AuthService
                    // The MainWindowViewModel will handle the UserLoggedInMessage
                    _logger?.LogInformation("User logged in successfully: {Username}", Username);
                    
                    // Clear sensitive data
                    Password = string.Empty;
                }
                else
                {
                    ErrorMessage = "Invalid username or password";
                    _logger?.LogWarning("Login failed for user: {Username}", Username);
                }
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation("Login operation was canceled");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Login failed. Please try again.";
                _logger?.LogError(ex, "Login failed for user: {Username}", Username);
                
                // Send error message for display
                WeakReferenceMessenger.Default.Send(new ErrorMessage(
                    $"Login failed: {ex.Message}",
                    "Authentication Error",
                    ErrorSeverity.Error));
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        public override void Cleanup()
        {
            // Cancel any pending operations and dispose resources
            _cts?.Cancel();
            _cts?.Dispose();
            
            // Clear sensitive data
            Password = string.Empty;
            
            base.Cleanup();
        }
    }
}