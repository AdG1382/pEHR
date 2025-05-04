using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging;
using EHRp.Messages;
using EHRp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EHRp.Services
{
    /// <summary>
    /// Implementation of the navigation service that handles view model navigation
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NavigationService> _logger;
        private readonly Stack<(Type ViewModelType, object? Parameter)> _navigationStack = new();
        
        /// <summary>
        /// Event that is raised when navigation occurs
        /// </summary>
        public event EventHandler<NavigationEventArgs>? Navigated;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService"/> class
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <param name="logger">The logger</param>
        public NavigationService(IServiceProvider serviceProvider, ILogger<NavigationService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Navigates to the specified view model type
        /// </summary>
        /// <typeparam name="T">The type of view model to navigate to</typeparam>
        public void NavigateTo<T>() where T : ViewModelBase
        {
            NavigateTo<T>(null);
        }
        
        /// <summary>
        /// Navigates to the specified view model type with parameters
        /// </summary>
        /// <typeparam name="T">The type of view model to navigate to</typeparam>
        /// <param name="parameter">The parameter to pass to the view model</param>
        public void NavigateTo<T>(object? parameter) where T : ViewModelBase
        {
            try
            {
                _logger.LogInformation("Navigating to {ViewModelType}", typeof(T).Name);
                
                // Get the view model from the service provider
                var viewModel = _serviceProvider.GetRequiredService<T>();
                
                // If the view model implements INavigationAware, call OnNavigatedTo
                if (viewModel is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedTo(parameter);
                }
                
                // Add the current view model to the navigation stack
                _navigationStack.Push((typeof(T), parameter));
                
                // Send a message to update the current view model
                WeakReferenceMessenger.Default.Send(new NavigationMessage(viewModel));
                
                // Raise the Navigated event
                Navigated?.Invoke(this, new NavigationEventArgs(viewModel, parameter));
                
                _logger.LogInformation("Successfully navigated to {ViewModelType}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to {ViewModelType}", typeof(T).Name);
                throw;
            }
        }
        
        /// <summary>
        /// Navigates back to the previous view model
        /// </summary>
        public void NavigateBack()
        {
            if (_navigationStack.Count <= 1)
            {
                _logger.LogWarning("Cannot navigate back, navigation stack is empty or has only one item");
                return;
            }
            
            // Remove the current view model from the stack
            _navigationStack.Pop();
            
            // Get the previous view model from the stack
            var (viewModelType, parameter) = _navigationStack.Peek();
            
            try
            {
                _logger.LogInformation("Navigating back to {ViewModelType}", viewModelType.Name);
                
                // Get the view model from the service provider
                var viewModel = (ViewModelBase)_serviceProvider.GetRequiredService(viewModelType);
                
                // If the view model implements INavigationAware, call OnNavigatedTo
                if (viewModel is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedTo(parameter);
                }
                
                // Send a message to update the current view model
                WeakReferenceMessenger.Default.Send(new NavigationMessage(viewModel));
                
                // Raise the Navigated event
                Navigated?.Invoke(this, new NavigationEventArgs(viewModel, parameter));
                
                _logger.LogInformation("Successfully navigated back to {ViewModelType}", viewModelType.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating back to {ViewModelType}", viewModelType.Name);
                throw;
            }
        }
    }
}