using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRp.Models
{
    public class Visit
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int PatientId { get; set; }
        
        [ForeignKey("PatientId")]
        public required virtual Patient Patient { get; set; }
        
        public DateTime VisitDate { get; set; }
        
        [StringLength(255)]
        public required string Reason { get; set; }
        
        [StringLength(1000)]
        public required string Notes { get; set; }
        
        [StringLength(255)]
        public required string Diagnosis { get; set; }
        
        [StringLength(500)]
        public required string Treatment { get; set; }
        
        public bool IsCompleted { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}