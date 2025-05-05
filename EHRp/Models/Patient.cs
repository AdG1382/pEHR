using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EHRp.Models
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string FirstName { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string LastName { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        
        [StringLength(10)]
        public required string Gender { get; set; }
        
        [StringLength(255)]
        public required string Address { get; set; }
        
        [StringLength(20)]
        public required string PhoneNumber { get; set; }
        
        [StringLength(255)]
        public required string Email { get; set; }
        
        [StringLength(255)]
        public string? InsuranceProvider { get; set; }
        
        [StringLength(50)]
        public string? InsuranceNumber { get; set; }
        
        [StringLength(255)]
        public string? MedicalHistory { get; set; }
        
        [StringLength(255)]
        public string? Allergies { get; set; }
        
        [StringLength(255)]
        public string? Notes { get; set; }
        
        // Path to patient's photo
        [StringLength(255)]
        public string? PhotoPath { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        public DateTime? LastVisitDate { get; set; }
        
        // Navigation properties
        public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public virtual ICollection<FileMetadata> Files { get; set; } = new List<FileMetadata>();
    }
}