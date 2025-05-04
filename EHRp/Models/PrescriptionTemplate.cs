using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRp.Models
{
    public class PrescriptionTemplate
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public required virtual User User { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string TemplateName { get; set; }
        
        [StringLength(1000)]
        public required string Medications { get; set; }
        
        [StringLength(1000)]
        public required string Instructions { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}