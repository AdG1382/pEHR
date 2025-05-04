using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EHRp.Services;
using Microsoft.Extensions.Logging;

namespace EHRp.ViewModels.Visits
{
    /// <summary>
    /// View model for adding or editing a visit
    /// </summary>
    public partial class VisitFormViewModel : ViewModelBase, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly ILogger<VisitFormViewModel> _logger;
        
        [ObservableProperty]
        private string _patientName = string.Empty;
        
        [ObservableProperty]
        private DateTime _visitDate = DateTime.Now;
        
        [ObservableProperty]
        private string _visitType = string.Empty;
        
        [ObservableProperty]
        private int _visitDuration = 30;
        
        [ObservableProperty]
        private string _notes = string.Empty;
        
        [ObservableProperty]
        private string _statusMessage = string.Empty;
        
        [ObservableProperty]
        private bool _isStatusVisible;
        
        [ObservableProperty]
        private bool _isStatusError;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="VisitFormViewModel"/> class
        /// </summary>
        /// <param name="navigationService">The navigation service</param>
        /// <param name="logger">The logger</param>
        public VisitFormViewModel(INavigationService navigationService, ILogger<VisitFormViewModel> logger)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Called when the view model is navigated to
        /// </summary>
        /// <param name="parameter">The parameter passed during navigation</param>
        public void OnNavigatedTo(object? parameter)
        {
            _logger.LogInformation("Navigated to VisitFormViewModel");
            
            // If parameter is a string, it's the patient name
            if (parameter is string patientName)
            {
                PatientName = patientName;
            }
            
            // Reset other form fields
            VisitDate = DateTime.Now;
            VisitType = string.Empty;
            VisitDuration = 30;
            Notes = string.Empty;
            
            // Clear status message
            StatusMessage = string.Empty;
            IsStatusVisible = false;
            IsStatusError = false;
        }
        
        /// <summary>
        /// Called when the view model is navigated from
        /// </summary>
        public void OnNavigatedFrom()
        {
            _logger.LogInformation("Navigated from VisitFormViewModel");
        }
        
        /// <summary>
        /// Saves the visit and returns to the previous view
        /// </summary>
        [RelayCommand]
        private void SaveVisit()
        {
            try
            {
                _logger.LogInformation("Saving visit for patient: {PatientName}", PatientName);
                
                // Validate form fields
                if (string.IsNullOrWhiteSpace(PatientName) || string.IsNullOrWhiteSpace(VisitType))
                {
                    ShowStatusMessage("Patient name and visit type are required", true);
                    return;
                }
                
                if (VisitDuration < 5 || VisitDuration > 240)
                {
                    ShowStatusMessage("Visit duration must be between 5 and 240 minutes", true);
                    return;
                }
                
                // TODO: Save visit to database
                
                // Show success message
                ShowStatusMessage($"Visit for {PatientName} added successfully");
                
                // Navigate back to the previous view
                _navigationService.NavigateBack();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving visit");
                ShowStatusMessage($"Error saving visit: {ex.Message}", true);
            }
        }
        
        /// <summary>
        /// Cancels the operation and returns to the previous view
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            _logger.LogInformation("Cancelling add visit operation");
            _navigationService.NavigateBack();
        }
        
        /// <summary>
        /// Shows a status message to the user
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <param name="isError">Whether the message is an error</param>
        private void ShowStatusMessage(string message, bool isError = false)
        {
            StatusMessage = message;
            IsStatusVisible = !string.IsNullOrEmpty(message);
            IsStatusError = isError;
            
            // Log the message
            if (isError)
            {
                _logger.LogError("Status message (error): {Message}", message);
            }
            else
            {
                _logger.LogInformation("Status message: {Message}", message);
            }
        }
    }
}