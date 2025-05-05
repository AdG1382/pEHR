using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace EHRp.ViewModels.Patients
{
    /// <summary>
    /// View model for a patient visit (simplified for UI display)
    /// </summary>
    public partial class VisitViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _id;
        
        [ObservableProperty]
        private int _patientId;
        
        [ObservableProperty]
        private DateTime _visitDate;
        
        [ObservableProperty]
        private string _reason = string.Empty;
        
        [ObservableProperty]
        private string _diagnosis = string.Empty;
        
        [ObservableProperty]
        private string _treatment = string.Empty;
        
        [ObservableProperty]
        private string _notes = string.Empty;
        
        [ObservableProperty]
        private bool _isCompleted;
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public VisitViewModel()
        {
        }
        
        /// <summary>
        /// Creates a new visit view model with the specified values
        /// </summary>
        public VisitViewModel(int id, int patientId, DateTime visitDate, string reason, string diagnosis, string treatment, string notes, bool isCompleted)
        {
            Id = id;
            PatientId = patientId;
            VisitDate = visitDate;
            Reason = reason ?? string.Empty;
            Diagnosis = diagnosis ?? string.Empty;
            Treatment = treatment ?? string.Empty;
            Notes = notes ?? string.Empty;
            IsCompleted = isCompleted;
        }
    }
}