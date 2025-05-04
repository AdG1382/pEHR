using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRp.Models
{
    public class FileMetadata
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int PatientId { get; set; }
        
        [ForeignKey("PatientId")]
        public required virtual Patient Patient { get; set; }
        
        [Required]
        [StringLength(255)]
        public required string FileName { get; set; }
        
        [Required]
        [StringLength(255)]
        public required string FilePath { get; set; }
        
        [StringLength(100)]
        public required string FileType { get; set; } // jpg, png, pdf, docx
        
        [StringLength(255)]
        public string? Description { get; set; }
        
        public bool IsEncrypted { get; set; }
        
        public long FileSize { get; set; }
        
        public DateTime UploadDate { get; set; } = DateTime.Now;
    }
}