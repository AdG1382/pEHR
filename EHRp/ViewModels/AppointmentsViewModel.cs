using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace EHRp.ViewModels
{
    public partial class AppointmentsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<AppointmentItem> _appointments = new ObservableCollection<AppointmentItem>();
        
        [ObservableProperty]
        private AppointmentItem? _selectedAppointment;
        
        [ObservableProperty]
        private string _searchText = "";
        
        [ObservableProperty]
        private DateTime _filterDate = DateTime.Today;
        
        [ObservableProperty]
        private string _filterType = "All";
        
        [ObservableProperty]
        private string _statusMessage = "";
        
        [ObservableProperty]
        private bool _isStatusSuccess;
        
        private readonly ILogger<AppointmentsViewModel>? _logger;
        
        // Default constructor for design-time support
        public AppointmentsViewModel()
        {
            // Load dummy data for now
            LoadDummyData();
        }
        
        // Constructor with logger for runtime
        public AppointmentsViewModel(ILogger<AppointmentsViewModel> logger) : this()
        {
            _logger = logger;
        }
        
        private void LoadDummyData()
        {
            // Add dummy appointments
            Appointments.Add(new AppointmentItem
            {
                Id = 1,
                PatientName = "John Doe",
                AppointmentDate = DateTime.Today,
                AppointmentTime = new TimeSpan(9, 0, 0),
                Duration = "30 min",
                AppointmentType = "Check-up",
                Status = "Scheduled",
                Notes = "Regular check-up",
                ColorCode = "#4CAF50" // Green
            });
            
            Appointments.Add(new AppointmentItem
            {
                Id = 2,
                PatientName = "Jane Smith",
                AppointmentDate = DateTime.Today,
                AppointmentTime = new TimeSpan(10, 30, 0),
                Duration = "30 min",
                AppointmentType = "Follow-up",
                Status = "Scheduled",
                Notes = "Follow-up on medication",
                ColorCode = "#2196F3" // Blue
            });
            
            Appointments.Add(new AppointmentItem
            {
                Id = 3,
                PatientName = "Robert Johnson",
                AppointmentDate = DateTime.Today,
                AppointmentTime = new TimeSpan(14, 0, 0),
                Duration = "45 min",
                AppointmentType = "Consultation",
                Status = "Scheduled",
                Notes = "New patient consultation",
                ColorCode = "#FF9800" // Orange
            });
            
            Appointments.Add(new AppointmentItem
            {
                Id = 4,
                PatientName = "Sarah Williams",
                AppointmentDate = DateTime.Today.AddDays(1),
                AppointmentTime = new TimeSpan(11, 0, 0),
                Duration = "60 min",
                AppointmentType = "Annual Physical",
                Status = "Scheduled",
                Notes = "Annual physical examination",
                ColorCode = "#9C27B0" // Purple
            });
            
            _logger?.LogInformation("Loaded {Count} appointments", Appointments.Count);
        }
        
        [RelayCommand]
        private void AddAppointment()
        {
            // This would open the add appointment dialog
            StatusMessage = "Add appointment feature not implemented yet";
            IsStatusSuccess = false;
            _logger?.LogInformation("Add appointment requested");
        }
        
        [RelayCommand]
        private void EditAppointment()
        {
            if (SelectedAppointment == null)
            {
                StatusMessage = "No appointment selected";
                IsStatusSuccess = false;
                return;
            }
            
            // This would open the edit appointment dialog
            StatusMessage = $"Edit appointment {SelectedAppointment.Id} feature not implemented yet";
            IsStatusSuccess = false;
            _logger?.LogInformation("Edit appointment requested for ID: {Id}", SelectedAppointment.Id);
        }
        
        [RelayCommand]
        private void DeleteAppointment()
        {
            if (SelectedAppointment == null)
            {
                StatusMessage = "No appointment selected";
                IsStatusSuccess = false;
                return;
            }
            
            // This would delete the appointment
            Appointments.Remove(SelectedAppointment);
            StatusMessage = $"Appointment {SelectedAppointment.Id} deleted";
            IsStatusSuccess = true;
            _logger?.LogInformation("Appointment deleted: {Id}", SelectedAppointment.Id);
            SelectedAppointment = null;
        }
        
        [RelayCommand]
        private void CancelAppointment()
        {
            if (SelectedAppointment == null)
            {
                StatusMessage = "No appointment selected";
                IsStatusSuccess = false;
                return;
            }
            
            // This would mark the appointment as cancelled
            SelectedAppointment.Status = "Cancelled";
            StatusMessage = $"Appointment {SelectedAppointment.Id} cancelled";
            IsStatusSuccess = true;
            _logger?.LogInformation("Appointment cancelled: {Id}", SelectedAppointment.Id);
        }
        
        [RelayCommand]
        private void MarkAsComplete()
        {
            if (SelectedAppointment == null)
            {
                StatusMessage = "No appointment selected";
                IsStatusSuccess = false;
                return;
            }
            
            // This would mark the appointment as complete
            SelectedAppointment.Status = "Completed";
            StatusMessage = $"Appointment {SelectedAppointment.Id} marked as completed";
            IsStatusSuccess = true;
            _logger?.LogInformation("Appointment marked as complete: {Id}", SelectedAppointment.Id);
        }
        
        [RelayCommand]
        private void Reschedule()
        {
            if (SelectedAppointment == null)
            {
                StatusMessage = "No appointment selected";
                IsStatusSuccess = false;
                return;
            }
            
            // This would open the reschedule dialog
            StatusMessage = $"Reschedule appointment {SelectedAppointment.Id} feature not implemented yet";
            IsStatusSuccess = false;
            _logger?.LogInformation("Reschedule requested for appointment: {Id}", SelectedAppointment.Id);
        }
        
        [RelayCommand]
        private void Search()
        {
            // This would filter the appointments based on search text
            StatusMessage = $"Search for '{SearchText}' feature not implemented yet";
            IsStatusSuccess = true;
            _logger?.LogInformation("Search requested with text: {SearchText}", SearchText);
        }
        
        [RelayCommand]
        private void Today()
        {
            // Set filter date to today
            FilterDate = DateTime.Today;
            StatusMessage = "Showing today's appointments";
            IsStatusSuccess = true;
            _logger?.LogInformation("Filter set to today: {Date}", FilterDate.ToShortDateString());
        }
        
        [RelayCommand]
        private void AllAppointments()
        {
            // Clear date filter
            StatusMessage = "Showing all appointments";
            IsStatusSuccess = true;
            _logger?.LogInformation("Showing all appointments");
        }
    }
    
    public class AppointmentItem
    {
        public int Id { get; set; }
        public required string PatientName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public required string Duration { get; set; }
        public required string AppointmentType { get; set; }
        public required string Status { get; set; }
        public string? Notes { get; set; }
        public required string ColorCode { get; set; }
    }
}