using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRp.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int PatientId { get; set; }
        
        [ForeignKey("PatientId")]
        public required virtual Patient Patient { get; set; }
        
        public DateTime AppointmentDate { get; set; }
        
        public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(30);
        
        [StringLength(100)]
        public required string Title { get; set; }
        
        [StringLength(255)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public required string AppointmentType { get; set; } // Follow-up, Surgery, Check-up, etc.
        
        public required string ColorCode { get; set; } // Color for calendar display
        
        public bool IsCompleted { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}