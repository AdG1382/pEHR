using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EHRp.Services;
using Microsoft.Extensions.Logging;

namespace EHRp.ViewModels.Prescriptions
{
    /// <summary>
    /// View model for adding or editing a prescription
    /// </summary>
    public partial class PrescriptionFormViewModel : ViewModelBase, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly ILogger<PrescriptionFormViewModel> _logger;
        
        [ObservableProperty]
        private string _patientName = string.Empty;
        
        [ObservableProperty]
        private string _medication = string.Empty;
        
        [ObservableProperty]
        private string _dosage = string.Empty;
        
        [ObservableProperty]
        private string _frequency = string.Empty;
        
        [ObservableProperty]
        private string _prescriptionType = "Regular";
        
        [ObservableProperty]
        private DateTime _startDate = DateTime.Now;
        
        [ObservableProperty]
        private DateTime _endDate = DateTime.Now.AddDays(30);
        
        [ObservableProperty]
        private string _notes = string.Empty;
        
        [ObservableProperty]
        private string _statusMessage = string.Empty;
        
        [ObservableProperty]
        private bool _isStatusVisible;
        
        [ObservableProperty]
        private bool _isStatusError;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PrescriptionFormViewModel"/> class
        /// </summary>
        /// <param name="navigationService">The navigation service</param>
        /// <param name="logger">The logger</param>
        public PrescriptionFormViewModel(INavigationService navigationService, ILogger<PrescriptionFormViewModel> logger)
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
            _logger.LogInformation("Navigated to PrescriptionFormViewModel");
            
            // If parameter is a string, it's the patient name
            if (parameter is string patientName)
            {
                PatientName = patientName;
            }
            
            // Reset other form fields
            Medication = string.Empty;
            Dosage = string.Empty;
            Frequency = string.Empty;
            PrescriptionType = "Regular";
            StartDate = DateTime.Now;
            EndDate = DateTime.Now.AddDays(30);
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
            _logger.LogInformation("Navigated from PrescriptionFormViewModel");
        }
        
        /// <summary>
        /// Saves the prescription and returns to the previous view
        /// </summary>
        [RelayCommand]
        private void SavePrescription()
        {
            try
            {
                _logger.LogInformation("Saving prescription for patient: {PatientName}", PatientName);
                
                // Validate form fields
                if (string.IsNullOrWhiteSpace(PatientName) || string.IsNullOrWhiteSpace(Medication) || 
                    string.IsNullOrWhiteSpace(Dosage) || string.IsNullOrWhiteSpace(Frequency))
                {
                    ShowStatusMessage("Patient name, medication, dosage, and frequency are required", true);
                    return;
                }
                
                if (EndDate < StartDate)
                {
                    ShowStatusMessage("End date cannot be earlier than start date", true);
                    return;
                }
                
                // TODO: Save prescription to database
                
                // Show success message
                ShowStatusMessage($"Prescription for {PatientName} added successfully");
                
                // Navigate back to the previous view
                _navigationService.NavigateBack();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving prescription");
                ShowStatusMessage($"Error saving prescription: {ex.Message}", true);
            }
        }
        
        /// <summary>
        /// Cancels the operation and returns to the previous view
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            _logger.LogInformation("Cancelling add prescription operation");
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