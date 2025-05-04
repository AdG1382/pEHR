using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRp.Models
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }
        
        public int? UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string ActivityType { get; set; } // Login, Logout, Create, Update, Delete, Backup, Error
        
        [StringLength(255)]
        public required string Description { get; set; }
        
        [StringLength(100)]
        public required string EntityType { get; set; } // Patient, Visit, Prescription, etc.
        
        public int? EntityId { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}