namespace EHRp.Constants
{
    /// <summary>
    /// Contains application-wide constant values.
    /// </summary>
    public static class AppConstants
    {
        /// <summary>
        /// Constants related to the calendar view.
        /// </summary>
        public static class CalendarView
        {
            /// <summary>
            /// Month view.
            /// </summary>
            public const string Month = "Month";
            
            /// <summary>
            /// Week view.
            /// </summary>
            public const string Week = "Week";
            
            /// <summary>
            /// Day view.
            /// </summary>
            public const string Day = "Day";
        }
        
        /// <summary>
        /// Constants related to appointment types.
        /// </summary>
        public static class AppointmentTypes
        {
            /// <summary>
            /// Check-up appointment.
            /// </summary>
            public const string CheckUp = "Check-up";
            
            /// <summary>
            /// Follow-up appointment.
            /// </summary>
            public const string FollowUp = "Follow-up";
            
            /// <summary>
            /// Consultation appointment.
            /// </summary>
            public const string Consultation = "Consultation";
            
            /// <summary>
            /// Surgery appointment.
            /// </summary>
            public const string Surgery = "Surgery";
            
            /// <summary>
            /// Annual physical appointment.
            /// </summary>
            public const string AnnualPhysical = "Annual Physical";
        }
        
        /// <summary>
        /// Constants related to color codes.
        /// </summary>
        public static class ColorCodes
        {
            /// <summary>
            /// Green color code.
            /// </summary>
            public const string Green = "#4CAF50";
            
            /// <summary>
            /// Blue color code.
            /// </summary>
            public const string Blue = "#2196F3";
            
            /// <summary>
            /// Orange color code.
            /// </summary>
            public const string Orange = "#FF9800";
            
            /// <summary>
            /// Purple color code.
            /// </summary>
            public const string Purple = "#9C27B0";
            
            /// <summary>
            /// Red color code.
            /// </summary>
            public const string Red = "#F44336";
        }
        
        /// <summary>
        /// Constants related to activity types.
        /// </summary>
        public static class ActivityTypes
        {
            /// <summary>
            /// Login activity.
            /// </summary>
            public const string Login = "Login";
            
            /// <summary>
            /// Logout activity.
            /// </summary>
            public const string Logout = "Logout";
            
            /// <summary>
            /// Create activity.
            /// </summary>
            public const string Create = "Create";
            
            /// <summary>
            /// Update activity.
            /// </summary>
            public const string Update = "Update";
            
            /// <summary>
            /// Delete activity.
            /// </summary>
            public const string Delete = "Delete";
            
            /// <summary>
            /// View activity.
            /// </summary>
            public const string View = "View";
        }
        
        /// <summary>
        /// Constants related to entity types.
        /// </summary>
        public static class EntityTypes
        {
            /// <summary>
            /// User entity.
            /// </summary>
            public const string User = "User";
            
            /// <summary>
            /// Patient entity.
            /// </summary>
            public const string Patient = "Patient";
            
            /// <summary>
            /// Appointment entity.
            /// </summary>
            public const string Appointment = "Appointment";
            
            /// <summary>
            /// Prescription entity.
            /// </summary>
            public const string Prescription = "Prescription";
            
            /// <summary>
            /// File entity.
            /// </summary>
            public const string File = "File";
            
            /// <summary>
            /// Visit entity.
            /// </summary>
            public const string Visit = "Visit";
        }
    }
}