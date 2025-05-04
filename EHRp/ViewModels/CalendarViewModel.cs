using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using EHRp.Constants;
using EHRp.Data;
using EHRp.Messages;
using EHRp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EHRp.ViewModels
{
    /// <summary>
    /// ViewModel for the calendar view that displays and manages appointments.
    /// </summary>
    public partial class CalendarViewModel : ViewModelBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CalendarViewModel> _logger;
        
        /// <summary>
        /// Gets or sets the selected date in the calendar.
        /// </summary>
        [ObservableProperty]
        private DateTime _selectedDate = DateTime.Today;
        
        /// <summary>
        /// Gets or sets the collection of calendar events.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<CalendarEvent> _events = new ObservableCollection<CalendarEvent>();
        
        /// <summary>
        /// Gets or sets the current view mode (Month, Week, Day).
        /// </summary>
        [ObservableProperty]
        private string _currentView = AppConstants.CalendarView.Month;
        
        /// <summary>
        /// Gets or sets a value indicating whether data is currently loading.
        /// </summary>
        [ObservableProperty]
        private bool _isLoading;
        
        /// <summary>
        /// Gets or sets the error message to display.
        /// </summary>
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarViewModel"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public CalendarViewModel(
            ApplicationDbContext context,
            ILogger<CalendarViewModel> logger)
        {
            _context = context;
            _logger = logger;
            
            // Register for appointment update messages
            WeakReferenceMessenger.Default.Register<AppointmentUpdatedMessage>(this, (r, m) => OnAppointmentUpdated(m));
            WeakReferenceMessenger.Default.Register<AppointmentDeletedMessage>(this, (r, m) => OnAppointmentDeleted(m));
            
            // Load data when the view model is created
            LoadEventsAsync().ConfigureAwait(false);
            
            // Set up property changed handler for selected date and current view
            PropertyChanged += (sender, args) => 
            {
                if (args.PropertyName == nameof(SelectedDate) || args.PropertyName == nameof(CurrentView))
                {
                    LoadEventsAsync().ConfigureAwait(false);
                }
            };
        }
        
        /// <summary>
        /// Handles cleanup when the view model is unloaded.
        /// </summary>
        public override void Cleanup()
        {
            // Unregister from messenger
            WeakReferenceMessenger.Default.UnregisterAll(this);
            base.Cleanup();
        }
        
        /// <summary>
        /// Loads events from the database based on the current view and selected date.
        /// </summary>
        private async Task LoadEventsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                // Calculate date range based on current view
                DateTime startDate, endDate;
                
                switch (CurrentView)
                {
                    case AppConstants.CalendarView.Month:
                        // Get first day of month
                        startDate = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
                        // Get last day of month
                        endDate = startDate.AddMonths(1).AddDays(-1);
                        break;
                    
                    case AppConstants.CalendarView.Week:
                        // Get first day of week (Sunday)
                        startDate = SelectedDate.AddDays(-(int)SelectedDate.DayOfWeek);
                        // Get last day of week (Saturday)
                        endDate = startDate.AddDays(6);
                        break;
                    
                    case AppConstants.CalendarView.Day:
                        startDate = SelectedDate.Date;
                        endDate = startDate.AddDays(1).AddSeconds(-1);
                        break;
                    
                    default:
                        startDate = SelectedDate.Date;
                        endDate = startDate.AddDays(1).AddSeconds(-1);
                        break;
                }
                
                _logger.LogDebug("Loading calendar events from {StartDate} to {EndDate}", startDate, endDate);
                
                // Query appointments from database with eager loading of patient data
                var appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
                    .OrderBy(a => a.AppointmentDate)
                    .AsNoTracking()
                    .ToListAsync();
                
                // Convert to calendar events
                var calendarEvents = appointments.Select(a => new CalendarEvent
                {
                    Id = a.Id,
                    Title = $"{a.Patient.FirstName} {a.Patient.LastName} - {a.Title}",
                    Start = a.AppointmentDate,
                    End = a.AppointmentDate.Add(a.Duration),
                    ColorCode = a.ColorCode,
                    PatientId = a.PatientId,
                    IsCompleted = a.IsCompleted,
                    Description = a.Description,
                    AppointmentType = a.AppointmentType
                }).ToList();
                
                // Update the observable collection on the UI thread
                await ExecuteOnUIThreadAsync(() =>
                {
                    Events.Clear();
                    foreach (var calendarEvent in calendarEvents)
                    {
                        Events.Add(calendarEvent);
                    }
                });
                
                _logger.LogInformation("Loaded {Count} calendar events", calendarEvents.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading calendar events");
                ErrorMessage = "Failed to load appointments. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// Handles the appointment updated message.
        /// </summary>
        /// <param name="message">The appointment updated message.</param>
        private void OnAppointmentUpdated(AppointmentUpdatedMessage message)
        {
            _logger.LogDebug("Received appointment updated message for appointment {AppointmentId}", message.Value.Id);
            
            // Reload events to reflect the changes
            LoadEventsAsync().ConfigureAwait(false);
        }
        
        /// <summary>
        /// Handles the appointment deleted message.
        /// </summary>
        /// <param name="message">The appointment deleted message.</param>
        private void OnAppointmentDeleted(AppointmentDeletedMessage message)
        {
            _logger.LogDebug("Received appointment deleted message for appointment {AppointmentId}", message.Value);
            
            // Remove the event from the collection if it exists
            var eventToRemove = Events.FirstOrDefault(e => e.Id == message.Value);
            if (eventToRemove != null)
            {
                ExecuteOnUIThread(() => Events.Remove(eventToRemove));
            }
        }
        
        /// <summary>
        /// Opens the add event dialog.
        /// </summary>
        [RelayCommand]
        private async Task AddEventAsync()
        {
            try
            {
                _logger.LogDebug("Opening add appointment dialog");
                
                // In a real implementation, this would open a dialog to add a new appointment
                // For now, we'll just log that this method was called
                
                // After the appointment is added, reload the events
                await LoadEventsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding appointment");
                ErrorMessage = "Failed to add appointment. Please try again.";
            }
        }
        
        /// <summary>
        /// Opens the edit event dialog for the specified event.
        /// </summary>
        /// <param name="eventId">The ID of the event to edit.</param>
        [RelayCommand]
        private async Task EditEventAsync(int eventId)
        {
            try
            {
                _logger.LogDebug("Opening edit appointment dialog for appointment {AppointmentId}", eventId);
                
                // In a real implementation, this would open a dialog to edit the appointment
                // For now, we'll just log that this method was called
                
                // After the appointment is edited, reload the events
                await LoadEventsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing appointment {AppointmentId}", eventId);
                ErrorMessage = "Failed to edit appointment. Please try again.";
            }
        }
        
        /// <summary>
        /// Deletes the specified event.
        /// </summary>
        /// <param name="eventId">The ID of the event to delete.</param>
        [RelayCommand]
        private async Task DeleteEventAsync(int eventId)
        {
            try
            {
                _logger.LogDebug("Deleting appointment {AppointmentId}", eventId);
                
                // Find the appointment in the database
                var appointment = await _context.Appointments.FindAsync(eventId);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment not found for deletion: {AppointmentId}", eventId);
                    ErrorMessage = "Appointment not found.";
                    return;
                }
                
                // Remove the appointment from the database
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                
                // Log the deletion
                var activityLog = new ActivityLog
                {
                    UserId = CurrentUser?.Id ?? 0, // Assuming CurrentUser is set in the base class
                    ActivityType = AppConstants.ActivityTypes.Delete,
                    Description = $"Deleted appointment: {appointment.Title}",
                    EntityType = AppConstants.EntityTypes.Appointment,
                    EntityId = eventId,
                    Timestamp = DateTime.Now
                };
                
                _context.ActivityLogs.Add(activityLog);
                await _context.SaveChangesAsync();
                
                // Remove the event from the collection
                var eventToRemove = Events.FirstOrDefault(e => e.Id == eventId);
                if (eventToRemove != null)
                {
                    Events.Remove(eventToRemove);
                }
                
                // Notify other view models that the appointment was deleted
                WeakReferenceMessenger.Default.Send(new AppointmentDeletedMessage(eventId));
                
                _logger.LogInformation("Deleted appointment {AppointmentId}", eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting appointment {AppointmentId}", eventId);
                ErrorMessage = "Failed to delete appointment. Please try again.";
            }
        }
        
        /// <summary>
        /// Changes the current view mode.
        /// </summary>
        /// <param name="view">The view mode to change to.</param>
        [RelayCommand]
        private void ChangeView(string view)
        {
            if (view != CurrentView)
            {
                _logger.LogDebug("Changing calendar view from {OldView} to {NewView}", CurrentView, view);
                CurrentView = view;
            }
        }
        
        /// <summary>
        /// Navigates to today's date.
        /// </summary>
        [RelayCommand]
        private void NavigateToToday()
        {
            _logger.LogDebug("Navigating to today's date");
            SelectedDate = DateTime.Today;
        }
        
        /// <summary>
        /// Navigates to the previous time period based on the current view.
        /// </summary>
        [RelayCommand]
        private void NavigateToPrevious()
        {
            switch (CurrentView)
            {
                case AppConstants.CalendarView.Month:
                    _logger.LogDebug("Navigating to previous month");
                    SelectedDate = SelectedDate.AddMonths(-1);
                    break;
                
                case AppConstants.CalendarView.Week:
                    _logger.LogDebug("Navigating to previous week");
                    SelectedDate = SelectedDate.AddDays(-7);
                    break;
                
                case AppConstants.CalendarView.Day:
                    _logger.LogDebug("Navigating to previous day");
                    SelectedDate = SelectedDate.AddDays(-1);
                    break;
            }
        }
        
        /// <summary>
        /// Navigates to the next time period based on the current view.
        /// </summary>
        [RelayCommand]
        private void NavigateToNext()
        {
            switch (CurrentView)
            {
                case AppConstants.CalendarView.Month:
                    _logger.LogDebug("Navigating to next month");
                    SelectedDate = SelectedDate.AddMonths(1);
                    break;
                
                case AppConstants.CalendarView.Week:
                    _logger.LogDebug("Navigating to next week");
                    SelectedDate = SelectedDate.AddDays(7);
                    break;
                
                case AppConstants.CalendarView.Day:
                    _logger.LogDebug("Navigating to next day");
                    SelectedDate = SelectedDate.AddDays(1);
                    break;
            }
        }
    }
    
    /// <summary>
    /// Represents an event in the calendar.
    /// </summary>
    public class CalendarEvent
    {
        /// <summary>
        /// Gets or sets the ID of the event.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the title of the event.
        /// </summary>
        public required string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the start time of the event.
        /// </summary>
        public DateTime Start { get; set; }
        
        /// <summary>
        /// Gets or sets the end time of the event.
        /// </summary>
        public DateTime End { get; set; }
        
        /// <summary>
        /// Gets or sets the color code for the event.
        /// </summary>
        public string? ColorCode { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the patient associated with the event.
        /// </summary>
        public int PatientId { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the appointment is completed.
        /// </summary>
        public bool IsCompleted { get; set; }
        
        /// <summary>
        /// Gets or sets the description of the event.
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the appointment.
        /// </summary>
        public string? AppointmentType { get; set; }
    }
}