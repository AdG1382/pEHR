using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using EHRp.Models;

namespace EHRp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<FileMetadata> FilesMetadata { get; set; }
        public DbSet<UserSetting> UserSettings { get; set; }
        public DbSet<PrescriptionTemplate> PrescriptionTemplates { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        
        public ApplicationDbContext()
        {
        }
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Use the project directory for the database during development
                string dbPath = "ehrp.db";
                
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure relationships and constraints
            modelBuilder.Entity<Patient>()
                .HasMany(p => p.Visits)
                .WithOne(v => v.Patient)
                .HasForeignKey(v => v.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Patient>()
                .HasMany(p => p.Prescriptions)
                .WithOne(pr => pr.Patient)
                .HasForeignKey(pr => pr.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Patient>()
                .HasMany(p => p.Files)
                .WithOne(f => f.Patient)
                .HasForeignKey(f => f.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}