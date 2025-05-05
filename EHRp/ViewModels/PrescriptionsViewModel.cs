using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace EHRp.ViewModels
{
    public partial class PrescriptionsViewModel : ViewModelBase, INavigationAware
    {
        [ObservableProperty]
        private ObservableCollection<PrescriptionItem> _prescriptions = new ObservableCollection<PrescriptionItem>();
        
        [ObservableProperty]
        private ObservableCollection<string> _filterOptions = new ObservableCollection<string>
        {
            "All",
            "Active",
            "Expired",
            "Cancelled"
        };
        
        [ObservableProperty]
        private string _selectedFilter = "All";
        
        [ObservableProperty]
        private string _searchText = "";
        
        [ObservableProperty]
        private PrescriptionItem? _selectedPrescription;
        
        [ObservableProperty]
        private string _statusMessage = "";
        
        [ObservableProperty]
        private bool _isStatusSuccess;
        
        private readonly ILogger<PrescriptionsViewModel>? _logger;
        
        // Default constructor for design-time support
        public PrescriptionsViewModel()
        {
            // Load dummy data for now
            LoadDummyData();
        }
        
        // Constructor with logger for runtime
        public PrescriptionsViewModel(ILogger<PrescriptionsViewModel> logger) : this()
        {
            _logger = logger;
        }
        
        private void LoadDummyData()
        {
            // Add dummy prescriptions
            Prescriptions.Add(new PrescriptionItem
            {
                Id = 1,
                PatientName = "John Doe",
                MedicationName = "Lisinopril",
                Dosage = "10mg",
                Frequency = "Once daily",
                StartDate = DateTime.Today.AddDays(-5),
                EndDate = DateTime.Today.AddDays(25),
                Status = "Active",
                Notes = "Take with water. Monitor blood pressure regularly."
            });
            
            Prescriptions.Add(new PrescriptionItem
            {
                Id = 2,
                PatientName = "John Doe",
                MedicationName = "Hydrochlorothiazide",
                Dosage = "12.5mg",
                Frequency = "Once daily",
                StartDate = DateTime.Today.AddDays(-5),
                EndDate = DateTime.Today.AddDays(25),
                Status = "Active",
                Notes = "Take with water in the morning."
            });
            
            Prescriptions.Add(new PrescriptionItem
            {
                Id = 3,
                PatientName = "Jane Smith",
                MedicationName = "Albuterol Inhaler",
                Dosage = "2 puffs",
                Frequency = "As needed",
                StartDate = DateTime.Today.AddDays(-10),
                EndDate = DateTime.Today.AddDays(80),
                Status = "Active",
                Notes = "Use for shortness of breath."
            });
            
            Prescriptions.Add(new PrescriptionItem
            {
                Id = 4,
                PatientName = "Jane Smith",
                MedicationName = "Fluticasone",
                Dosage = "50mcg",
                Frequency = "Twice daily",
                StartDate = DateTime.Today.AddDays(-10),
                EndDate = DateTime.Today.AddDays(80),
                Status = "Active",
                Notes = "Use morning and evening."
            });
            
            Prescriptions.Add(new PrescriptionItem
            {
                Id = 5,
                PatientName = "Robert Johnson",
                MedicationName = "Metformin",
                Dosage = "500mg",
                Frequency = "Twice daily",
                StartDate = DateTime.Today.AddDays(-30),
                EndDate = DateTime.Today.AddDays(-1),
                Status = "Expired",
                Notes = "Take with meals."
            });
            
            _logger?.LogInformation("Loaded {Count} prescriptions", Prescriptions.Count);
        }
        
        [RelayCommand]
        private void NewPrescription()
        {
            // This would open the new prescription editor
            StatusMessage = "New prescription feature not implemented yet";
            IsStatusSuccess = false;
            _logger?.LogInformation("New prescription requested");
        }
        
        [RelayCommand]
        private void EditPrescription()
        {
            if (SelectedPrescription == null)
            {
                StatusMessage = "No prescription selected";
                IsStatusSuccess = false;
                return;
            }
            
            // This would open the edit prescription editor
            StatusMessage = $"Edit prescription {SelectedPrescription.Id} feature not implemented yet";
            IsStatusSuccess = false;
            _logger?.LogInformation("Edit prescription requested for ID: {Id}", SelectedPrescription.Id);
        }
        
        [RelayCommand]
        private void DeletePrescription()
        {
            if (SelectedPrescription == null)
            {
                StatusMessage = "No prescription selected";
                IsStatusSuccess = false;
                return;
            }
            
            // This would delete the prescription
            Prescriptions.Remove(SelectedPrescription);
            StatusMessage = $"Prescription {SelectedPrescription.Id} deleted";
            IsStatusSuccess = true;
            _logger?.LogInformation("Prescription deleted: {Id}", SelectedPrescription.Id);
            SelectedPrescription = null;
        }
        
        [RelayCommand]
        private void PrintPrescription()
        {
            if (SelectedPrescription == null)
            {
                StatusMessage = "No prescription selected";
                IsStatusSuccess = false;
                return;
            }
            
            // This would print the prescription
            StatusMessage = $"Print prescription {SelectedPrescription.Id} feature not implemented yet";
            IsStatusSuccess = false;
            _logger?.LogInformation("Print requested for prescription: {Id}", SelectedPrescription.Id);
        }
        
        [RelayCommand]
        private void Search()
        {
            // This would filter the prescriptions based on search text and selected filter
            StatusMessage = $"Search for '{SearchText}' with filter '{SelectedFilter}' feature not implemented yet";
            IsStatusSuccess = true;
            _logger?.LogInformation("Search requested with text: {SearchText} and filter: {Filter}", SearchText, SelectedFilter);
        }
        
        partial void OnSelectedFilterChanged(string value)
        {
            // This would filter the prescriptions based on the selected filter
            _logger?.LogInformation("Filter changed to: {Filter}", value);
            Search();
        }
        
        /// <summary>
        /// Called when the view model is navigated to
        /// </summary>
        /// <param name="parameter">The parameter passed during navigation</param>
        public void OnNavigatedTo(object? parameter)
        {
            _logger?.LogInformation("Navigated to PrescriptionsViewModel with parameter: {Parameter}", parameter);
            
            try
            {
                // Reset any error states
                StatusMessage = "";
                IsStatusSuccess = true;
                
                // Ensure we have valid data
                if (Prescriptions == null)
                {
                    Prescriptions = new ObservableCollection<PrescriptionItem>();
                    LoadDummyData();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in OnNavigatedTo for PrescriptionsViewModel");
                StatusMessage = $"Error loading prescriptions: {ex.Message}";
                IsStatusSuccess = false;
            }
        }
        
        /// <summary>
        /// Called when the view model is navigated from
        /// </summary>
        public void OnNavigatedFrom()
        {
            _logger?.LogInformation("Navigated from PrescriptionsViewModel");
            // Clean up any resources if needed
        }
    }
    
    public class PrescriptionItem
    {
        public int Id { get; set; }
        public required string PatientName { get; set; }
        public required string MedicationName { get; set; }
        public required string Dosage { get; set; }
        public required string Frequency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public required string Status { get; set; }
        public string? Notes { get; set; }
    }
}