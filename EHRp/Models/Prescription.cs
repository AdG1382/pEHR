using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRp.Models
{
    public class Prescription
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int PatientId { get; set; }
        
        [ForeignKey("PatientId")]
        public required virtual Patient Patient { get; set; }
        
        public DateTime PrescriptionDate { get; set; } = DateTime.Now;
        
        [StringLength(1000)]
        public required string Medications { get; set; }
        
        [StringLength(1000)]
        public string? Instructions { get; set; }
        
        [StringLength(255)]
        public string? PdfPath { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}