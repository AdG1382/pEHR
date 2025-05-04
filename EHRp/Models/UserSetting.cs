using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRp.Models
{
    public class UserSetting
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public required virtual User User { get; set; }
        
        [StringLength(50)]
        public required string Theme { get; set; } = "Light"; // Light or Dark
        
        [StringLength(255)]
        public required string BackupPath { get; set; }
        
        public bool AutoBackup { get; set; }
        
        public int BackupFrequencyDays { get; set; } = 7;
        
        public DateTime LastBackupDate { get; set; } = DateTime.Now;
        
        [StringLength(50)]
        public string FontSize { get; set; } = "Medium"; // Small, Medium, Large
        
        [StringLength(50)]
        public string ButtonStyle { get; set; } = "Rounded"; // Rounded or Flat
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}