using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EHRp.Services;
using Microsoft.Extensions.Logging;

namespace EHRp.ViewModels.Patients
{
    /// <summary>
    /// View model for adding a new patient
    /// </summary>
    public partial class AddPatientViewModel : ViewModelBase, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly ILogger<AddPatientViewModel> _logger;
        
        [ObservableProperty]
        private string _firstName = string.Empty;
        
        [ObservableProperty]
        private string _lastName = string.Empty;
        
        [ObservableProperty]
        private DateTime _dateOfBirth = DateTime.Now.AddYears(-30);
        
        [ObservableProperty]
        private string _gender = "Male";
        
        [ObservableProperty]
        private string _email = string.Empty;
        
        [ObservableProperty]
        private string _phone = string.Empty;
        
        [ObservableProperty]
        private string _address = string.Empty;
        
        [ObservableProperty]
        private string _notes = string.Empty;
        
        [ObservableProperty]
        private string _statusMessage = string.Empty;
        
        [ObservableProperty]
        private bool _isStatusVisible;
        
        [ObservableProperty]
        private bool _isStatusError;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AddPatientViewModel"/> class
        /// </summary>
        /// <param name="navigationService">The navigation service</param>
        /// <param name="logger">The logger</param>
        public AddPatientViewModel(INavigationService navigationService, ILogger<AddPatientViewModel> logger)
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
            _logger.LogInformation("Navigated to AddPatientViewModel");
            
            // Reset form fields
            FirstName = string.Empty;
            LastName = string.Empty;
            DateOfBirth = DateTime.Now.AddYears(-30);
            Gender = "Male";
            Email = string.Empty;
            Phone = string.Empty;
            Address = string.Empty;
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
            _logger.LogInformation("Navigated from AddPatientViewModel");
        }
        
        /// <summary>
        /// Saves the patient and returns to the previous view
        /// </summary>
        [RelayCommand]
        private void SavePatient()
        {
            try
            {
                _logger.LogInformation("Saving patient: {FirstName} {LastName}", FirstName, LastName);
                
                // Validate form fields
                if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
                {
                    ShowStatusMessage("First name and last name are required", true);
                    return;
                }
                
                // TODO: Save patient to database
                
                // Show success message
                ShowStatusMessage($"Patient {FirstName} {LastName} added successfully");
                
                // Navigate back to the previous view
                _navigationService.NavigateBack();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving patient");
                ShowStatusMessage($"Error saving patient: {ex.Message}", true);
            }
        }
        
        /// <summary>
        /// Cancels the operation and returns to the previous view
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            _logger.LogInformation("Cancelling add patient operation");
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