using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EHRp.Services;
using EHRp.ViewModels.Patients;
using EHRp.ViewModels.Prescriptions;
using EHRp.ViewModels.Visits;
using Microsoft.Extensions.Logging;

namespace EHRp.ViewModels
{
    public partial class DashboardViewModel : ViewModelBase, INavigationAware
    {
        private readonly INavigationService? _navigationService;
        private readonly ILogger<DashboardViewModel>? _logger;
        
        [ObservableProperty]
        private ObservableCollection<AppointmentSummary> _todaysAppointments = new ObservableCollection<AppointmentSummary>();
        
        [ObservableProperty]
        private int _totalPatients;
        
        [ObservableProperty]
        private int _appointmentsThisWeek;
        
        [ObservableProperty]
        private int _pendingPrescriptions;
        
        [ObservableProperty]
        private string _statusMessage = "No command executed yet.";
        
        // Default constructor for design-time support
        public DashboardViewModel()
        {
            // Load dummy data for now
            LoadDummyData();
            Debug.WriteLine("DashboardViewModel created with default constructor");
        }
        
        // Constructor with dependencies
        public DashboardViewModel(INavigationService navigationService, ILogger<DashboardViewModel> logger) : this()
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _logger.LogInformation("DashboardViewModel created with dependencies");
        }
        
        private void LoadDummyData()
        {
            // Add dummy appointments
            TodaysAppointments.Add(new AppointmentSummary
            {
                PatientName = "John Doe",
                Time = "09:00 AM",
                AppointmentType = "Check-up",
                ColorCode = "#4CAF50" // Green
            });
            
            TodaysAppointments.Add(new AppointmentSummary
            {
                PatientName = "Jane Smith",
                Time = "10:30 AM",
                AppointmentType = "Follow-up",
                ColorCode = "#2196F3" // Blue
            });
            
            TodaysAppointments.Add(new AppointmentSummary
            {
                PatientName = "Robert Johnson",
                Time = "02:00 PM",
                AppointmentType = "Consultation",
                ColorCode = "#FF9800" // Orange
            });
            
            // Set dummy stats
            TotalPatients = 125;
            AppointmentsThisWeek = 15;
            PendingPrescriptions = 7;
        }
        
        public void OnNavigatedTo(object? parameter)
        {
            _logger?.LogInformation("Navigated to DashboardViewModel");
            
            // Refresh data if needed
            // For now, we'll just use the dummy data
        }
        
        public void OnNavigatedFrom()
        {
            _logger?.LogInformation("Navigated from DashboardViewModel");
        }
        
        [RelayCommand]
        private void AddPatient()
        {
            try
            {
                _logger?.LogInformation("Executing AddPatient command");
                
                // Update status message
                StatusMessage = $"Navigating to Add Patient at {DateTime.Now.ToLongTimeString()}";
                
                // Navigate to the add patient view
                _navigationService?.NavigateTo<AddPatientViewModel>();
                
                _logger?.LogInformation("Successfully navigated to AddPatientViewModel");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing AddPatient command");
                StatusMessage = $"Error: {ex.Message}";
            }
        }
        
        [RelayCommand]
        private void AddVisit()
        {
            try
            {
                _logger?.LogInformation("Executing AddVisit command");
                
                // Update status message
                StatusMessage = $"Navigating to Add Visit at {DateTime.Now.ToLongTimeString()}";
                
                // Navigate to the add visit view
                _navigationService?.NavigateTo<VisitFormViewModel>();
                
                _logger?.LogInformation("Successfully navigated to VisitFormViewModel");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing AddVisit command");
                StatusMessage = $"Error: {ex.Message}";
            }
        }
        
        [RelayCommand]
        private void NewPrescription()
        {
            try
            {
                _logger?.LogInformation("Executing NewPrescription command");
                
                // Update status message
                StatusMessage = $"Navigating to New Prescription at {DateTime.Now.ToLongTimeString()}";
                
                // Navigate to the new prescription view
                _navigationService?.NavigateTo<PrescriptionFormViewModel>();
                
                _logger?.LogInformation("Successfully navigated to PrescriptionFormViewModel");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing NewPrescription command");
                StatusMessage = $"Error: {ex.Message}";
            }
        }
        
        [RelayCommand]
        private void ViewPatient(string patientName)
        {
            try
            {
                _logger?.LogInformation("Executing ViewPatient command for {PatientName}", patientName);
                
                // Update status message
                StatusMessage = $"Viewing patient {patientName} at {DateTime.Now.ToLongTimeString()}";
                
                // TODO: Navigate to the patient details view
                // For now, we'll just log it
                _logger?.LogInformation("ViewPatient navigation not yet implemented for {PatientName}", patientName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing ViewPatient command for {PatientName}", patientName);
                StatusMessage = $"Error: {ex.Message}";
            }
        }
    }
    
    public class AppointmentSummary
    {
        public string PatientName { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string AppointmentType { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
    }
}