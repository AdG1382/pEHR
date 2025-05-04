using System;
using System.ComponentModel.DataAnnotations;

namespace EHRp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string Username { get; set; }
        
        [Required]
        public required string PasswordHash { get; set; }
        
        [StringLength(255)]
        public required string FullName { get; set; }
        
        [StringLength(255)]
        public required string Email { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? LastLogin { get; set; }
    }
}