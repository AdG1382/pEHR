using System;
using EHRp.ViewModels;

namespace EHRp.Services
{
    /// <summary>
    /// Interface for navigation service that handles view model navigation
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigates to the specified view model type
        /// </summary>
        /// <typeparam name="T">The type of view model to navigate to</typeparam>
        void NavigateTo<T>() where T : ViewModelBase;
        
        /// <summary>
        /// Navigates to the specified view model type with parameters
        /// </summary>
        /// <typeparam name="T">The type of view model to navigate to</typeparam>
        /// <param name="parameter">The parameter to pass to the view model</param>
        void NavigateTo<T>(object parameter) where T : ViewModelBase;
        
        /// <summary>
        /// Navigates back to the previous view model
        /// </summary>
        void NavigateBack();
        
        /// <summary>
        /// Event that is raised when navigation occurs
        /// </summary>
        event EventHandler<NavigationEventArgs> Navigated;
    }
    
    /// <summary>
    /// Event arguments for navigation events
    /// </summary>
    public class NavigationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the view model that was navigated to
        /// </summary>
        public ViewModelBase ViewModel { get; }
        
        /// <summary>
        /// Gets the parameter that was passed to the view model
        /// </summary>
        public object? Parameter { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationEventArgs"/> class
        /// </summary>
        /// <param name="viewModel">The view model that was navigated to</param>
        /// <param name="parameter">The parameter that was passed to the view model</param>
        public NavigationEventArgs(ViewModelBase viewModel, object? parameter = null)
        {
            ViewModel = viewModel;
            Parameter = parameter;
        }
    }
}