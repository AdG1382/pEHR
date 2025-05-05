using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EHRp.Models;
using EHRp.Services;
using EHRp.ViewModels.Patients;
using Microsoft.Extensions.Logging;

namespace EHRp.ViewModels.Patients
{
    /// <summary>
    /// View model for displaying patient details
    /// </summary>
    public partial class PatientDetailViewModel : ViewModelBase, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly IPatientService _patientService;
        private readonly ILogger<PatientDetailViewModel> _logger;
        
        [ObservableProperty]
        private Patient? _patient;
        
        [ObservableProperty]
        private bool _isLoading;
        
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        private int _selectedTabIndex;
        
        /// <summary>
        /// Gets or sets the selected tab index
        /// </summary>
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                try
                {
                    // Validate the tab index to prevent crashes
                    if (value < 0 || value > 3)
                    {
                        _logger.LogWarning("Invalid tab index: {TabIndex}", value);
                        return;
                    }
                    
                    // If switching to the Visits tab (index 2), ensure we have valid data
                    if (value == 2)
                    {
                        _logger.LogInformation("Switching to Visits tab");
                        
                        try
                        {
                            // Ensure we have a valid collection
                            if (PatientVisits == null)
                            {
                                PatientVisits = new ObservableCollection<VisitViewModel>();
                            }
                            
                            // Make sure HasVisits is properly set based on collection content
                            HasVisits = PatientVisits.Count > 0;
                            
                            // If we have a patient ID but no visits loaded yet, try to load them
                            if (Patient != null && PatientVisits.Count == 0)
                            {
                                // We'll use Task.Run to avoid blocking the UI thread
                                // This is not ideal but will prevent crashes
                                Task.Run(async () => 
                                {
                                    try 
                                    {
                                        await LoadPatientVisitsAsync(Patient.Id);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Error loading visits in background for patient ID: {PatientId}", Patient.Id);
                                    }
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error preparing Visits tab");
                            // Ensure we have a valid collection even after an error
                            PatientVisits = new ObservableCollection<VisitViewModel>();
                            HasVisits = false;
                        }
                    }
                    
                    // If switching to the Lab Reports tab (index 3), ensure we have valid data
                    if (value == 3)
                    {
                        _logger.LogInformation("Switching to Lab Reports tab");
                        
                        try
                        {
                            // Ensure we have a valid collection
                            if (LabReports == null)
                            {
                                LabReports = new ObservableCollection<FileMetadata>();
                            }
                            
                            // Make sure HasLabReports is properly set based on collection content
                            HasLabReports = LabReports.Count > 0;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error preparing Lab Reports tab");
                            // Ensure we have a valid collection even after an error
                            LabReports = new ObservableCollection<FileMetadata>();
                            HasLabReports = false;
                        }
                    }
                    
                    // Set the value and notify property changed
                    SetProperty(ref _selectedTabIndex, value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting selected tab index to {TabIndex}", value);
                    ErrorMessage = $"Error switching tabs: {ex.Message}";
                    
                    // Default to the Personal tab (index 0) in case of error
                    SetProperty(ref _selectedTabIndex, 0);
                }
            }
        }
        
        [ObservableProperty]
        private string _patientHeaderText = "Patient Details";
        
        [ObservableProperty]
        private string _patientFullName = string.Empty;
        
        [ObservableProperty]
        private int _calculatedAge;
        
        [ObservableProperty]
        private string _patientPhotoPath = string.Empty;
        
        [ObservableProperty]
        private string _insuranceDisplay = "Not specified";
        
        [ObservableProperty]
        private string _chronicIllnesses = "-";
        
        [ObservableProperty]
        private string _currentMedications = "-";
        
        [ObservableProperty]
        private string _pastSurgeries = "-";
        
        [ObservableProperty]
        private string _ongoingTherapies = "-";
        
        [ObservableProperty]
        private ObservableCollection<VisitViewModel> _patientVisits = new ObservableCollection<VisitViewModel>();
        
        [ObservableProperty]
        private bool _hasVisits;
        
        [ObservableProperty]
        private ObservableCollection<FileMetadata> _labReports = new ObservableCollection<FileMetadata>();
        
        [ObservableProperty]
        private bool _hasLabReports;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PatientDetailViewModel"/> class
        /// </summary>
        /// <param name="navigationService">The navigation service</param>
        /// <param name="patientService">The patient service</param>
        /// <param name="logger">The logger</param>
        public PatientDetailViewModel(
            INavigationService navigationService,
            IPatientService patientService,
            ILogger<PatientDetailViewModel> logger)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Default constructor for design-time support
        /// </summary>
        public PatientDetailViewModel()
        {
            // This constructor is used by the designer
            LoadDummyData();
        }
        
        /// <summary>
        /// Called when the view model is navigated to
        /// </summary>
        /// <param name="parameter">The parameter passed during navigation</param>
        public async void OnNavigatedTo(object? parameter)
        {
            _logger.LogInformation("Navigated to PatientDetailViewModel with parameter: {Parameter}", parameter);
            
            // Reset state
            IsLoading = true;
            ErrorMessage = string.Empty;
            SelectedTabIndex = 0;
            
            // Initialize empty collections to avoid null reference errors
            if (PatientVisits == null)
            {
                PatientVisits = new ObservableCollection<VisitViewModel>();
            }
            else
            {
                PatientVisits.Clear();
            }
            
            if (LabReports == null)
            {
                LabReports = new ObservableCollection<FileMetadata>();
            }
            else
            {
                LabReports.Clear();
            }
            
            HasVisits = false;
            HasLabReports = false;
            
            try
            {
                int patientId = -1;
                
                // Handle different parameter types
                if (parameter is int id)
                {
                    patientId = id;
                }
                else if (parameter is PatientListItem patientListItem)
                {
                    patientId = patientListItem.Id;
                }
                
                if (patientId <= 0)
                {
                    ErrorMessage = "Invalid patient ID";
                    return;
                }
                
                // Load patient data
                await LoadPatientDataAsync(patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading patient details");
                ErrorMessage = $"Error loading patient details: {ex.Message}";
                
                // Ensure we have empty collections rather than null
                if (PatientVisits == null)
                {
                    PatientVisits = new ObservableCollection<VisitViewModel>();
                }
                
                if (LabReports == null)
                {
                    LabReports = new ObservableCollection<FileMetadata>();
                }
                
                HasVisits = false;
                HasLabReports = false;
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// Called when the view model is navigated from
        /// </summary>
        public void OnNavigatedFrom()
        {
            _logger.LogInformation("Navigated from PatientDetailViewModel");
        }
        
        /// <summary>
        /// Loads patient data from the service
        /// </summary>
        /// <param name="patientId">The patient ID</param>
        private async Task LoadPatientDataAsync(int patientId)
        {
            try
            {
                // Load patient details
                var patient = await _patientService.GetPatientByIdAsync(patientId);
                
                if (patient == null)
                {
                    ErrorMessage = "Patient not found";
                    return;
                }
                
                Patient = patient;
                
                // Update derived properties with null checks
                PatientFullName = $"{patient.FirstName ?? "Unknown"} {patient.LastName ?? ""}".Trim();
                PatientHeaderText = $"Patient: {PatientFullName}";
                CalculatedAge = CalculateAge(patient.DateOfBirth);
                
                // Set default photo path (could be updated with actual photo path)
                PatientPhotoPath = string.IsNullOrEmpty(patient.PhotoPath) ? 
                    "/Assets/default-profile.png" : patient.PhotoPath;
                
                // Set insurance display
                if (!string.IsNullOrEmpty(patient.InsuranceProvider))
                {
                    InsuranceDisplay = !string.IsNullOrEmpty(patient.InsuranceNumber) ?
                        $"{patient.InsuranceProvider} (#{patient.InsuranceNumber})" :
                        patient.InsuranceProvider;
                }
                else
                {
                    InsuranceDisplay = "Not specified";
                }
                
                // Load visits
                await LoadPatientVisitsAsync(patientId);
                
                // Load lab reports
                await LoadLabReportsAsync(patientId);
                
                _logger.LogInformation("Successfully loaded patient data for ID: {PatientId}", patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading patient data for ID: {PatientId}", patientId);
                ErrorMessage = $"Error loading patient data: {ex.Message}";
                
                // Initialize empty collections to avoid null reference errors
                if (PatientVisits == null)
                {
                    PatientVisits = new ObservableCollection<VisitViewModel>();
                }
                
                if (LabReports == null)
                {
                    LabReports = new ObservableCollection<FileMetadata>();
                }
                
                HasVisits = false;
                HasLabReports = false;
            }
        }
        
        /// <summary>
        /// Loads patient visits from the service
        /// </summary>
        /// <param name="patientId">The patient ID</param>
        private async Task LoadPatientVisitsAsync(int patientId)
        {
            try
            {
                _logger.LogInformation("Loading visits for patient ID: {PatientId}", patientId);
                
                // Initialize the collection if it's null
                if (PatientVisits == null)
                {
                    PatientVisits = new ObservableCollection<VisitViewModel>();
                }
                else
                {
                    // Clear the existing collection
                    PatientVisits.Clear();
                }
                
                // TODO: Implement loading visits from a service
                // For now, we'll add dummy data for testing
                if (Patient != null)
                {
                    try
                    {
                        // Safely add dummy data with proper null checks and error handling
                        try
                        {
                            // Add dummy data for testing - for even patient IDs
                            if (patientId % 2 == 0)
                            {
                                PatientVisits.Add(new VisitViewModel(
                                    id: 1,
                                    patientId: patientId,
                                    visitDate: DateTime.Now.AddMonths(-1),
                                    reason: "Annual checkup",
                                    diagnosis: "Healthy",
                                    treatment: "None required",
                                    notes: "Patient is in good health",
                                    isCompleted: true
                                ));
                                
                                PatientVisits.Add(new VisitViewModel(
                                    id: 2,
                                    patientId: patientId,
                                    visitDate: DateTime.Now.AddMonths(-6),
                                    reason: "Flu symptoms",
                                    diagnosis: "Seasonal flu",
                                    treatment: "Rest and fluids",
                                    notes: "Follow up in 2 weeks if symptoms persist",
                                    isCompleted: true
                                ));
                            }
                            // For odd patient IDs, we'll add a single visit
                            else
                            {
                                PatientVisits.Add(new VisitViewModel(
                                    id: 3,
                                    patientId: patientId,
                                    visitDate: DateTime.Now.AddMonths(-3),
                                    reason: "Headache",
                                    diagnosis: "Tension headache",
                                    treatment: "Pain relievers and rest",
                                    notes: "Patient reported recurring headaches",
                                    isCompleted: true
                                ));
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error creating specific visit item for patient ID: {PatientId}", patientId);
                            // Continue execution to ensure we at least have an empty collection
                        }
                        
                        _logger.LogInformation("Successfully added dummy visits for patient ID: {PatientId}", patientId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating dummy visit data for patient ID: {PatientId}", patientId);
                        // Clear any partial data that might have been added
                        PatientVisits.Clear();
                    }
                }
                
                // Ensure we have a valid collection even if everything above fails
                if (PatientVisits == null)
                {
                    PatientVisits = new ObservableCollection<VisitViewModel>();
                }
                
                // Update the HasVisits property
                HasVisits = PatientVisits.Count > 0;
                
                _logger.LogInformation("Loaded {Count} visits for patient ID: {PatientId}", PatientVisits.Count, patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading patient visits for ID: {PatientId}", patientId);
                // Don't throw here, just log the error
                
                // Ensure we have a valid collection even after an error
                if (PatientVisits == null)
                {
                    PatientVisits = new ObservableCollection<VisitViewModel>();
                }
                HasVisits = false;
                
                // Ensure we have an empty collection rather than null
                if (PatientVisits == null)
                {
                    PatientVisits = new ObservableCollection<VisitViewModel>();
                }
                
                HasVisits = false;
                ErrorMessage = $"Error loading visits: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Loads lab reports from the service
        /// </summary>
        /// <param name="patientId">The patient ID</param>
        private async Task LoadLabReportsAsync(int patientId)
        {
            try
            {
                // Initialize the collection if it's null
                if (LabReports == null)
                {
                    LabReports = new ObservableCollection<FileMetadata>();
                }
                else
                {
                    // Clear the existing collection
                    LabReports.Clear();
                }
                
                // TODO: Implement loading lab reports from a service
                // For now, we'll add dummy data for testing
                if (Patient != null)
                {
                    // Add dummy data for testing - for patients with ID divisible by 3
                    if (patientId % 3 == 0)
                    {
                        LabReports.Add(new FileMetadata
                        {
                            Id = 1,
                            PatientId = patientId,
                            Patient = Patient,
                            FileName = "blood_test.pdf",
                            FileType = "PDF",
                            Description = "Complete Blood Count",
                            UploadDate = DateTime.Now.AddMonths(-1),
                            FilePath = "/Files/lab_reports/blood_test.pdf"
                        });
                        
                        LabReports.Add(new FileMetadata
                        {
                            Id = 2,
                            PatientId = patientId,
                            Patient = Patient,
                            FileName = "xray.jpg",
                            FileType = "Image",
                            Description = "Chest X-Ray",
                            UploadDate = DateTime.Now.AddMonths(-2),
                            FilePath = "/Files/lab_reports/xray.jpg"
                        });
                    }
                    // For patients with ID divisible by 5, add different reports
                    else if (patientId % 5 == 0)
                    {
                        LabReports.Add(new FileMetadata
                        {
                            Id = 3,
                            PatientId = patientId,
                            Patient = Patient,
                            FileName = "mri_scan.pdf",
                            FileType = "PDF",
                            Description = "MRI Scan Results",
                            UploadDate = DateTime.Now.AddMonths(-3),
                            FilePath = "/Files/lab_reports/mri_scan.pdf"
                        });
                    }
                }
                
                // Update the HasLabReports property
                HasLabReports = LabReports.Count > 0;
                
                _logger.LogInformation("Loaded {Count} lab reports for patient ID: {PatientId}", LabReports.Count, patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading lab reports for ID: {PatientId}", patientId);
                // Don't throw here, just log the error
                
                // Ensure we have an empty collection rather than null
                if (LabReports == null)
                {
                    LabReports = new ObservableCollection<FileMetadata>();
                }
                
                HasLabReports = false;
            }
        }
        
        /// <summary>
        /// Calculates age from date of birth
        /// </summary>
        /// <param name="birthDate">The date of birth</param>
        /// <returns>The calculated age</returns>
        private int CalculateAge(DateTime? birthDate)
        {
            if (!birthDate.HasValue)
                return 0;
            
            try
            {
                var today = DateTime.Today;
                var age = today.Year - birthDate.Value.Year;
                if (birthDate.Value.Date > today.AddYears(-age))
                    age--;
                    
                return age;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating age from birth date: {BirthDate}", birthDate);
                return 0;
            }
        }
        
        /// <summary>
        /// Changes the selected tab
        /// </summary>
        /// <param name="tabIndex">The tab index</param>
        [RelayCommand]
        private void ChangeTab(string tabIndex)
        {
            try
            {
                if (int.TryParse(tabIndex, out int index))
                {
                    _logger.LogInformation("Changing tab to index: {TabIndex}", index);
                    
                    // If switching to the Visits tab (index 2), ensure we have valid data
                    if (index == 2)
                    {
                        _logger.LogInformation("Switching to Visits tab");
                        
                        // Ensure we have a valid collection
                        if (PatientVisits == null)
                        {
                            PatientVisits = new ObservableCollection<VisitViewModel>();
                            HasVisits = false;
                        }
                    }
                    
                    // If switching to the Lab Reports tab (index 3), ensure we have valid data
                    if (index == 3)
                    {
                        _logger.LogInformation("Switching to Lab Reports tab");
                        
                        // Ensure we have a valid collection
                        if (LabReports == null)
                        {
                            LabReports = new ObservableCollection<FileMetadata>();
                            HasLabReports = false;
                        }
                    }
                    
                    SelectedTabIndex = index;
                    _logger.LogInformation("Changed tab to index: {TabIndex}", index);
                }
                else
                {
                    _logger.LogWarning("Invalid tab index string: {TabIndex}", tabIndex);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing tab to index: {TabIndex}", tabIndex);
                ErrorMessage = $"Error changing tab: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Navigates back to the previous view
        /// </summary>
        [RelayCommand]
        private void GoBack()
        {
            _logger.LogInformation("Navigating back from patient details");
            _navigationService.NavigateBack();
        }
        
        /// <summary>
        /// Edits the patient
        /// </summary>
        [RelayCommand]
        private void EditPatient()
        {
            if (Patient == null)
                return;
                
            _logger.LogInformation("Edit patient action triggered for patient ID: {PatientId}", Patient.Id);
            
            // For now, just show a message since EditPatientViewModel is not implemented yet
            // TODO: Implement EditPatientViewModel and uncomment the navigation code
            ErrorMessage = "Edit patient functionality will be implemented in a future update";
            
            // Once EditPatientViewModel is implemented, use this code:
            /*
            try
            {
                // Navigate to the edit patient view with the current patient as parameter
                _navigationService.NavigateTo<EditPatientViewModel>(Patient);
                
                // Clear any error messages
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to edit patient view for patient ID: {PatientId}", Patient.Id);
                ErrorMessage = $"Error opening edit patient form: {ex.Message}";
            }
            */
        }
        
        /// <summary>
        /// Adds a visit for the patient
        /// </summary>
        [RelayCommand]
        private void AddVisit()
        {
            if (Patient == null)
                return;
                
            _logger.LogInformation("Add visit action triggered for patient ID: {PatientId}", Patient.Id);
            
            // For now, just show a message since AddVisitViewModel is not implemented yet
            // TODO: Implement AddVisitViewModel and uncomment the navigation code
            ErrorMessage = "Add visit functionality will be implemented in a future update";
            
            // Once AddVisitViewModel is implemented, use this code:
            /*
            try
            {
                // Navigate to the add visit view with the current patient as parameter
                _navigationService.NavigateTo<AddVisitViewModel>(Patient);
                
                // Clear any error messages
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to add visit view for patient ID: {PatientId}", Patient.Id);
                ErrorMessage = $"Error opening add visit form: {ex.Message}";
            }
            */
        }
        
        /// <summary>
        /// Adds a prescription for the patient
        /// </summary>
        [RelayCommand]
        private void AddPrescription()
        {
            if (Patient == null)
                return;
                
            _logger.LogInformation("Add prescription action triggered for patient ID: {PatientId}", Patient.Id);
            
            // For now, just show a message since AddPrescriptionViewModel is not implemented yet
            // TODO: Implement AddPrescriptionViewModel and uncomment the navigation code
            ErrorMessage = "Add prescription functionality will be implemented in a future update";
            
            // Once AddPrescriptionViewModel is implemented, use this code:
            /*
            try
            {
                // Navigate to the add prescription view with the current patient as parameter
                _navigationService.NavigateTo<AddPrescriptionViewModel>(Patient);
                
                // Clear any error messages
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to add prescription view for patient ID: {PatientId}", Patient.Id);
                ErrorMessage = $"Error opening add prescription form: {ex.Message}";
            }
            */
        }
        
        /// <summary>
        /// Views a lab report
        /// </summary>
        /// <param name="report">The lab report</param>
        [RelayCommand]
        private void ViewLabReport(FileMetadata report)
        {
            if (report == null)
            {
                _logger.LogWarning("Attempted to view a null lab report");
                ErrorMessage = "Error: Cannot view report (report data is missing)";
                return;
            }
                
            _logger.LogInformation("View lab report action triggered for report ID: {ReportId}", report.Id);
            
            try
            {
                // TODO: Implement viewing lab reports
                // For now, just show a message with the report details
                ErrorMessage = $"Viewing report: {report.Description} ({report.FileType}) - This functionality will be implemented in a future update";
                
                // In a real implementation, we would:
                // 1. Check if the file exists at report.FilePath
                // 2. Open the file using the appropriate viewer based on report.FileType
                // 3. Log the access for audit purposes
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing lab report with ID: {ReportId}", report.Id);
                ErrorMessage = $"Error viewing report: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Loads dummy data for design-time support
        /// </summary>
        private void LoadDummyData()
        {
            try
            {
                // Initialize collections
                if (PatientVisits == null)
                {
                    PatientVisits = new ObservableCollection<VisitViewModel>();
                }
                else
                {
                    PatientVisits.Clear();
                }
                
                if (LabReports == null)
                {
                    LabReports = new ObservableCollection<FileMetadata>();
                }
                else
                {
                    LabReports.Clear();
                }
                
                // Create dummy patient
                Patient = new Patient
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    DateOfBirth = DateTime.Now.AddYears(-45),
                    Gender = "Male",
                    PhoneNumber = "555-123-4567",
                    Email = "john.doe@example.com",
                    Address = "123 Main St, Anytown, USA",
                    MedicalHistory = "No significant medical history",
                    Allergies = "None",
                    InsuranceProvider = "HealthPlus",
                    InsuranceNumber = "HP12345678"
                };
                
                PatientFullName = $"{Patient.FirstName} {Patient.LastName}";
                PatientHeaderText = $"Patient: {PatientFullName}";
                CalculatedAge = CalculateAge(Patient.DateOfBirth);
                PatientPhotoPath = "/Assets/default-profile.png";
                InsuranceDisplay = $"{Patient.InsuranceProvider} (#{Patient.InsuranceNumber})";
                
                // Add dummy visit
                PatientVisits.Add(new VisitViewModel(
                    id: 1,
                    patientId: 1,
                    visitDate: DateTime.Now.AddMonths(-1),
                    reason: "Annual checkup",
                    diagnosis: "Healthy",
                    treatment: "None required",
                    notes: "Patient is in good health",
                    isCompleted: true
                ));
                
                // Add dummy lab report
                LabReports.Add(new FileMetadata
                {
                    Id = 1,
                    PatientId = 1,
                    Patient = Patient,
                    FileName = "blood_test.pdf",
                    FileType = "PDF",
                    Description = "Complete Blood Count",
                    UploadDate = DateTime.Now.AddMonths(-1),
                    FilePath = "/Files/lab_reports/blood_test.pdf"
                });
                
                // Update state properties
                HasVisits = PatientVisits.Count > 0;
                HasLabReports = LabReports.Count > 0;
            }
            catch (Exception ex)
            {
                // In design-time, we can't log, but we can set the error message
                ErrorMessage = $"Error loading dummy data: {ex.Message}";
                
                // Ensure we have empty collections
                PatientVisits = new ObservableCollection<VisitViewModel>();
                LabReports = new ObservableCollection<FileMetadata>();
                HasVisits = false;
                HasLabReports = false;
            }
        }

    }
}
