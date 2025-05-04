using System;
using CommunityToolkit.Mvvm.Messaging.Messages;
using EHRp.Models;

namespace EHRp.Messages
{
    /// <summary>
    /// Message sent when the application theme is changed.
    /// </summary>
    public class ThemeChangedMessage : ValueChangedMessage<(string Theme, string Mode)>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeChangedMessage"/> class.
        /// </summary>
        /// <param name="theme">The theme that was selected.</param>
        /// <param name="mode">The mode (Light/Dark) that was selected.</param>
        public ThemeChangedMessage(string theme, string mode) 
            : base((theme ?? throw new ArgumentNullException(nameof(theme)), 
                   mode ?? throw new ArgumentNullException(nameof(mode))))
        {
        }
    }
    /// <summary>
    /// Message sent when a user successfully logs in.
    /// </summary>
    public class UserLoggedInMessage : ValueChangedMessage<User>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserLoggedInMessage"/> class.
        /// </summary>
        /// <param name="user">The user that logged in.</param>
        public UserLoggedInMessage(User user) : base(user ?? throw new ArgumentNullException(nameof(user)))
        {
        }
    }

    /// <summary>
    /// Message sent when a user logs out.
    /// </summary>
    public class UserLoggedOutMessage : ValueChangedMessage<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserLoggedOutMessage"/> class.
        /// </summary>
        /// <param name="userId">The ID of the user that logged out.</param>
        public UserLoggedOutMessage(int userId) : base(userId)
        {
        }
    }

    /// <summary>
    /// Message sent when a patient is created or updated.
    /// </summary>
    public class PatientUpdatedMessage : ValueChangedMessage<Patient>
    {
        /// <summary>
        /// Gets a value indicating whether the patient was newly created.
        /// </summary>
        public bool IsNewPatient { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientUpdatedMessage"/> class.
        /// </summary>
        /// <param name="patient">The patient that was updated.</param>
        /// <param name="isNewPatient">A value indicating whether the patient was newly created.</param>
        public PatientUpdatedMessage(Patient patient, bool isNewPatient) 
            : base(patient ?? throw new ArgumentNullException(nameof(patient)))
        {
            IsNewPatient = isNewPatient;
        }
    }

    /// <summary>
    /// Message sent when an appointment is created or updated.
    /// </summary>
    public class AppointmentUpdatedMessage : ValueChangedMessage<Appointment>
    {
        /// <summary>
        /// Gets a value indicating whether the appointment was newly created.
        /// </summary>
        public bool IsNewAppointment { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppointmentUpdatedMessage"/> class.
        /// </summary>
        /// <param name="appointment">The appointment that was updated.</param>
        /// <param name="isNewAppointment">A value indicating whether the appointment was newly created.</param>
        public AppointmentUpdatedMessage(Appointment appointment, bool isNewAppointment) 
            : base(appointment ?? throw new ArgumentNullException(nameof(appointment)))
        {
            IsNewAppointment = isNewAppointment;
        }
    }

    /// <summary>
    /// Message sent when an appointment is deleted.
    /// </summary>
    public class AppointmentDeletedMessage : ValueChangedMessage<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppointmentDeletedMessage"/> class.
        /// </summary>
        /// <param name="appointmentId">The ID of the appointment that was deleted.</param>
        public AppointmentDeletedMessage(int appointmentId) : base(appointmentId)
        {
        }
    }

    /// <summary>
    /// Message sent when a prescription is created or updated.
    /// </summary>
    public class PrescriptionUpdatedMessage : ValueChangedMessage<Prescription>
    {
        /// <summary>
        /// Gets a value indicating whether the prescription was newly created.
        /// </summary>
        public bool IsNewPrescription { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrescriptionUpdatedMessage"/> class.
        /// </summary>
        /// <param name="prescription">The prescription that was updated.</param>
        /// <param name="isNewPrescription">A value indicating whether the prescription was newly created.</param>
        public PrescriptionUpdatedMessage(Prescription prescription, bool isNewPrescription) 
            : base(prescription ?? throw new ArgumentNullException(nameof(prescription)))
        {
            IsNewPrescription = isNewPrescription;
        }
    }
    
    /// <summary>
    /// Message sent to request navigation to a patient details view.
    /// </summary>
    public class NavigateToPatientDetailsMessage : ValueChangedMessage<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigateToPatientDetailsMessage"/> class.
        /// </summary>
        /// <param name="patientId">The ID of the patient to view.</param>
        public NavigateToPatientDetailsMessage(int patientId) : base(patientId)
        {
        }
    }
    
    /// <summary>
    /// Message sent to request navigation to an appointment details view.
    /// </summary>
    public class NavigateToAppointmentDetailsMessage : ValueChangedMessage<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigateToAppointmentDetailsMessage"/> class.
        /// </summary>
        /// <param name="appointmentId">The ID of the appointment to view.</param>
        public NavigateToAppointmentDetailsMessage(int appointmentId) : base(appointmentId)
        {
        }
    }
    
    /// <summary>
    /// Message sent to request navigation to a prescription details view.
    /// </summary>
    public class NavigateToPrescriptionDetailsMessage : ValueChangedMessage<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigateToPrescriptionDetailsMessage"/> class.
        /// </summary>
        /// <param name="prescriptionId">The ID of the prescription to view.</param>
        public NavigateToPrescriptionDetailsMessage(int prescriptionId) : base(prescriptionId)
        {
        }
    }
    
    /// <summary>
    /// Message sent to notify that an error occurred.
    /// </summary>
    public class ErrorMessage : ValueChangedMessage<string>
    {
        /// <summary>
        /// Gets the title of the error.
        /// </summary>
        public string Title { get; }
        
        /// <summary>
        /// Gets the severity of the error.
        /// </summary>
        public ErrorSeverity Severity { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="title">The title of the error.</param>
        /// <param name="severity">The severity of the error.</param>
        public ErrorMessage(string message, string title = "Error", ErrorSeverity severity = ErrorSeverity.Error) 
            : base(message ?? throw new ArgumentNullException(nameof(message)))
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Severity = severity;
        }
    }
    
    /// <summary>
    /// Represents the severity of an error.
    /// </summary>
    public enum ErrorSeverity
    {
        /// <summary>
        /// Information message.
        /// </summary>
        Information,
        
        /// <summary>
        /// Warning message.
        /// </summary>
        Warning,
        
        /// <summary>
        /// Error message.
        /// </summary>
        Error,
        
        /// <summary>
        /// Critical error message.
        /// </summary>
        Critical
    }
}