﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using EHRp.Messages;
using EHRp.Models;
using EHRp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EHRp.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly IServiceProvider? _serviceProvider;
        private readonly IAuthService? _authService;
        private readonly INavigationService? _navigationService;
        private readonly ILogger<MainWindowViewModel>? _logger;
        
        [ObservableProperty]
        private ViewModelBase? _currentViewModel;
        
        [ObservableProperty]
        private bool _isLoggedIn;
        
        [ObservableProperty]
        private bool _isSidebarCollapsed;
        
        [ObservableProperty]
        private string _selectedMenuItem = "Dashboard";
        
        [ObservableProperty]
        private User? _currentUser;
        
        public ObservableCollection<string> MenuItems { get; } = new ObservableCollection<string>
        {
            "Dashboard",
            "Patients",
            "Calendar",
            "Appointments",
            "Prescriptions",
            "Settings",
            "Maintenance",
            "Exit"
        };
        
        // Default constructor for design-time support
        public MainWindowViewModel()
        {
            // Set default selected menu item
            SelectedMenuItem = "Dashboard";
        }
        
        public MainWindowViewModel(
            IServiceProvider serviceProvider,
            IAuthService authService,
            INavigationService navigationService,
            ILogger<MainWindowViewModel> logger) : this()
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Set default view model
            CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
            
            // Register for messages
            WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, (r, m) => {
                CurrentUser = m.Value;
                IsLoggedIn = true;
                NavigateToDashboard();
            });
            
            WeakReferenceMessenger.Default.Register<UserLoggedOutMessage>(this, (r, m) => {
                IsLoggedIn = false;
                CurrentUser = default;
                CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
            });
            
            // Register for navigation messages
            WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) => {
                CurrentViewModel = m.Value;
            });
        }
        
        [ObservableProperty]
        private string _statusMessage = "";
        
        [ObservableProperty]
        private bool _isStatusVisible;
        
        [ObservableProperty]
        private bool _isStatusError;
        
        /// <summary>
        /// Navigates to the selected menu item with robust error handling
        /// </summary>
        [RelayCommand]
        private void NavigateToMenuItem(string menuItem)
        {
            if (menuItem == "Exit")
            {
                // Handle exit
                LogoutCommand.Execute(null);
                Environment.Exit(0);
                return;
            }
            
            SelectedMenuItem = menuItem;
            _logger?.LogInformation("Navigating to menu item: {MenuItem}", menuItem);
            
            // Clear any previous status messages
            StatusMessage = "";
            IsStatusVisible = false;
            
            try
            {
                switch (menuItem)
                {
                    case "Dashboard":
                        NavigateToDashboard();
                        break;
                    case "Patients":
                        _navigationService?.NavigateTo<PatientsViewModel>();
                        break;
                    case "Calendar":
                        _navigationService?.NavigateTo<CalendarViewModel>();
                        break;
                    case "Appointments":
                        _navigationService?.NavigateTo<AppointmentsViewModel>();
                        break;
                    case "Prescriptions":
                        _navigationService?.NavigateTo<PrescriptionsViewModel>();
                        break;
                    case "Settings":
                        _navigationService?.NavigateTo<SettingsViewModel>();
                        break;
                    case "Maintenance":
                        _navigationService?.NavigateTo<MaintenanceViewModel>();
                        break;
                    default:
                        _logger?.LogWarning("Unknown menu item: {MenuItem}", menuItem);
                        ShowStatusMessage($"Unknown menu item: {menuItem}", true);
                        return;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error navigating to menu item: {MenuItem}", menuItem);
                ShowStatusMessage($"Error loading {menuItem} module: {ex.Message}", true);
                
                // Fallback to a safe state - return to Dashboard
                try
                {
                    _logger?.LogInformation("Falling back to Dashboard due to navigation error");
                    NavigateToDashboard();
                }
                catch (Exception fallbackEx)
                {
                    _logger?.LogError(fallbackEx, "Critical error: Failed to fallback to Dashboard");
                    // As a last resort, create a simple error view model
                    CurrentViewModel = new ViewModelBase();
                    ShowStatusMessage("Critical error: Unable to load any module. Please restart the application.", true);
                }
            }
        }
        
        /// <summary>
        /// Shows a status message to the user
        /// </summary>
        private void ShowStatusMessage(string message, bool isError = false)
        {
            StatusMessage = message;
            IsStatusVisible = !string.IsNullOrEmpty(message);
            IsStatusError = isError;
            
            // Log the message
            if (isError)
            {
                _logger?.LogError("Status message (error): {Message}", message);
            }
            else
            {
                _logger?.LogInformation("Status message: {Message}", message);
            }
        }
        
        private void NavigateToDashboard()
        {
            try
            {
                _navigationService?.NavigateTo<DashboardViewModel>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error navigating to Dashboard via navigation service, falling back to direct navigation");
                
                try
                {
                    var dashboardViewModel = _serviceProvider?.GetRequiredService<DashboardViewModel>();
                    if (dashboardViewModel != null)
                    {
                        _logger?.LogInformation("Navigating to Dashboard with ViewModel type: {ViewModelType}", dashboardViewModel.GetType().Name);
                        CurrentViewModel = dashboardViewModel;
                    }
                    else
                    {
                        throw new InvalidOperationException("Failed to get DashboardViewModel from service provider");
                    }
                }
                catch (Exception fallbackEx)
                {
                    _logger?.LogError(fallbackEx, "Error navigating to Dashboard, creating fallback instance");
                    // Create a fallback instance directly
                    var fallbackViewModel = new DashboardViewModel();
                    _logger?.LogInformation("Created fallback DashboardViewModel");
                    CurrentViewModel = fallbackViewModel;
                }
            }
        }
        
        [RelayCommand]
        private async Task LogoutAsync()
        {
            if (CurrentUser != null)
            {
                try
                {
                    await _authService.LogoutAsync(CurrentUser.Id);
                    
                    // The UserLoggedOutMessage will be sent by the AuthService
                    // and handled by the registered message handler
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error during logout for user: {UserId}", CurrentUser.Id);
                    // Show error to user via status message
                    ShowStatusMessage($"Error during logout: {ex.Message}", true);
                }
            }
        }
        
        [RelayCommand]
        private void ToggleSidebar()
        {
            IsSidebarCollapsed = !IsSidebarCollapsed;
        }
        
        public override void Cleanup()
        {
            // Unregister from messages when this ViewModel is no longer needed
            WeakReferenceMessenger.Default.Unregister<UserLoggedInMessage>(this);
            WeakReferenceMessenger.Default.Unregister<UserLoggedOutMessage>(this);
            WeakReferenceMessenger.Default.Unregister<NavigationMessage>(this);
            
            base.Cleanup();
        }
    }
}
