using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using EHRp.Models;
using EHRp.Services;
using EHRp.ViewModels.Patients;
using Microsoft.Extensions.Logging;

namespace EHRp.ViewModels
{
    public partial class PatientsViewModel : ViewModelBase
    {
        private readonly IPatientService? _patientService;
        private readonly INavigationService? _navigationService;
        private readonly ILogger<PatientsViewModel>? _logger;
        
        [ObservableProperty]
        private ObservableCollection<PatientListItem> _patients = new ObservableCollection<PatientListItem>();
        
        [ObservableProperty]
        private string _searchText = "";
        
        [ObservableProperty]
        private PatientListItem? _selectedPatient;
        
        [ObservableProperty]
        private bool _isLoading;
        
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        // Default constructor for design-time support
        public PatientsViewModel()
        {
            // This constructor is used by the designer and for testing
            // Load dummy data for design-time view
            LoadDummyData();
        }
        
        public PatientsViewModel(
            IPatientService patientService, 
            INavigationService navigationService,
            ILogger<PatientsViewModel> logger)
        {
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Load data asynchronously when the ViewModel is created
            LoadPatientsAsync().ConfigureAwait(false);
        }
        
        private async Task LoadPatientsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                var patients = await _patientService.GetAllPatientsAsync();
                
                await ExecuteOnUIThreadAsync(() => {
                    Patients.Clear();
                    foreach (var patient in patients)
                    {
                        Patients.Add(new PatientListItem
                        {
                            Id = patient.Id,
                            FullName = $"{patient.FirstName} {patient.LastName}",
                            Age = CalculateAge(patient.DateOfBirth),
                            Gender = patient.Gender,
                            PhoneNumber = patient.PhoneNumber,
                            LastVisit = patient.LastVisitDate?.ToString("yyyy-MM-dd") ?? "Never"
                        });
                    }
                });
                
                _logger.LogInformation("Loaded {Count} patients", patients.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading patients");
                ErrorMessage = "Failed to load patients. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        [RelayCommand]
        private async Task SearchPatientsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadPatientsAsync();
                return;
            }
            
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                var searchResults = await _patientService.SearchPatientsAsync(SearchText);
                
                await ExecuteOnUIThreadAsync(() => {
                    Patients.Clear();
                    foreach (var patient in searchResults)
                    {
                        Patients.Add(new PatientListItem
                        {
                            Id = patient.Id,
                            FullName = $"{patient.FirstName} {patient.LastName}",
                            Age = CalculateAge(patient.DateOfBirth),
                            Gender = patient.Gender,
                            PhoneNumber = patient.PhoneNumber,
                            LastVisit = patient.LastVisitDate?.ToString("yyyy-MM-dd") ?? "Never"
                        });
                    }
                });
                
                _logger.LogInformation("Found {Count} patients matching search term: {SearchTerm}", 
                    searchResults.Count, SearchText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching patients with term: {SearchTerm}", SearchText);
                ErrorMessage = "Failed to search patients. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        [RelayCommand]
        private void AddNewPatient()
        {
            _logger.LogInformation("Add new patient action triggered");
            
            try
            {
                // Navigate to the AddPatientViewModel
                _navigationService.NavigateTo<EHRp.ViewModels.Patients.AddPatientViewModel>();
                
                // Clear any previous error messages
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to add patient view");
                ErrorMessage = $"Failed to open add patient form: {ex.Message}";
                
                // Log additional details for debugging
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception details");
                }
            }
        }
        
        [RelayCommand]
        private void ViewPatientDetailsAsync(int patientId)
        {
            // Navigate to the patient details page
            _logger.LogInformation("View patient details action triggered for patient ID: {PatientId}", patientId);
            
            // Use the navigation service to navigate to the patient details view
            _navigationService.NavigateTo<EHRp.ViewModels.Patients.PatientDetailViewModel>(patientId);
        }
        
        [RelayCommand]
        private void OpenPatient()
        {
            if (SelectedPatient != null)
            {
                _logger.LogInformation("Open patient action triggered for patient ID: {PatientId}", SelectedPatient.Id);
                
                try
                {
                    // Pass the entire patient object instead of just the ID
                    // This gives the PatientDetailViewModel more flexibility in handling the parameter
                    _navigationService.NavigateTo<EHRp.ViewModels.Patients.PatientDetailViewModel>(SelectedPatient);
                    
                    // Clear any previous error messages
                    ErrorMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error navigating to patient details for patient ID: {PatientId}", SelectedPatient.Id);
                    ErrorMessage = $"Failed to open patient details: {ex.Message}";
                    
                    // Log additional details for debugging
                    if (ex.InnerException != null)
                    {
                        _logger.LogError(ex.InnerException, "Inner exception details");
                    }
                }
            }
        }
        
        private int CalculateAge(DateTime? birthDate)
        {
            if (!birthDate.HasValue)
                return 0;
                
            var today = DateTime.Today;
            var age = today.Year - birthDate.Value.Year;
            if (birthDate.Value.Date > today.AddYears(-age))
                age--;
                
            return age;
        }
        
        private void LoadDummyData()
        {
            // Add dummy patients for design-time or testing
            Patients.Add(new PatientListItem
            {
                Id = 1,
                FullName = "John Doe",
                Age = 42,
                Gender = "Male",
                PhoneNumber = "555-123-4567",
                LastVisit = "2023-04-15"
            });
            
            Patients.Add(new PatientListItem
            {
                Id = 2,
                FullName = "Jane Smith",
                Age = 35,
                Gender = "Female",
                PhoneNumber = "555-987-6543",
                LastVisit = "2023-05-02"
            });
            
            Patients.Add(new PatientListItem
            {
                Id = 3,
                FullName = "Robert Johnson",
                Age = 58,
                Gender = "Male",
                PhoneNumber = "555-456-7890",
                LastVisit = "2023-03-20"
            });
        }
    }
    
    // Make this class public so it can be accessed from other view models
    public class PatientListItem
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string LastVisit { get; set; } = string.Empty;
        
        public override string ToString()
        {
            return $"PatientListItem: {Id} - {FullName}";
        }
    }
}