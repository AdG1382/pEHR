using System;
using System.Linq;
using EHRp.Models;
using Microsoft.EntityFrameworkCore;

namespace EHRp.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Create database if it doesn't exist
            context.Database.EnsureCreated();
            
            // Check if there are any users
            if (context.Users.Any())
            {
                return; // DB has been seeded
            }
            
            // Create default user
            var defaultUser = new User
            {
                Username = "doctor",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                FullName = "Default Doctor",
                Email = "doctor@example.com",
                CreatedAt = DateTime.Now
            };
            
            context.Users.Add(defaultUser);
            context.SaveChanges();
            
            // Create default user settings
            var userSettings = new UserSetting
            {
                UserId = defaultUser.Id,
                User = defaultUser,
                Theme = "Light",
                BackupPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                AutoBackup = true,
                BackupFrequencyDays = 7,
                LastBackupDate = DateTime.Now,
                FontSize = "Medium",
                ButtonStyle = "Rounded",
                UpdatedAt = DateTime.Now
            };
            
            context.UserSettings.Add(userSettings);
            
            // Add sample patients
            var patients = new Patient[]
            {
                new Patient
                {
                    FirstName = "John",
                    LastName = "Doe",
                    DateOfBirth = new DateTime(1980, 1, 15),
                    Gender = "Male",
                    Address = "123 Main St, Anytown, USA",
                    PhoneNumber = "555-123-4567",
                    Email = "john.doe@example.com",
                    MedicalHistory = "Hypertension, Diabetes",
                    Notes = "Regular checkup every 3 months",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new Patient
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    DateOfBirth = new DateTime(1975, 5, 20),
                    Gender = "Female",
                    Address = "456 Oak Ave, Somewhere, USA",
                    PhoneNumber = "555-987-6543",
                    Email = "jane.smith@example.com",
                    MedicalHistory = "Asthma",
                    Notes = "Allergic to penicillin",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };
            
            context.Patients.AddRange(patients);
            context.SaveChanges();
            
            // Add sample appointments
            var appointments = new Appointment[]
            {
                new Appointment
                {
                    PatientId = patients[0].Id,
                    Patient = patients[0],
                    AppointmentDate = DateTime.Now.AddDays(7),
                    Duration = TimeSpan.FromMinutes(30),
                    Title = "Follow-up Checkup",
                    Description = "Blood pressure monitoring",
                    AppointmentType = "Follow-up",
                    ColorCode = "#4CAF50", // Green
                    IsCompleted = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new Appointment
                {
                    PatientId = patients[1].Id,
                    Patient = patients[1],
                    AppointmentDate = DateTime.Now.AddDays(3),
                    Duration = TimeSpan.FromMinutes(45),
                    Title = "Annual Physical",
                    Description = "Complete health checkup",
                    AppointmentType = "Check-up",
                    ColorCode = "#2196F3", // Blue
                    IsCompleted = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };
            
            context.Appointments.AddRange(appointments);
            context.SaveChanges();
            
            // Add sample prescription templates
            var prescriptionTemplates = new PrescriptionTemplate[]
            {
                new PrescriptionTemplate
                {
                    UserId = defaultUser.Id,
                    User = defaultUser,
                    TemplateName = "Hypertension Standard",
                    Medications = "Lisinopril 10mg\nHydrochlorothiazide 12.5mg",
                    Instructions = "Take once daily with water. Monitor blood pressure regularly.",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new PrescriptionTemplate
                {
                    UserId = defaultUser.Id,
                    User = defaultUser,
                    TemplateName = "Diabetes Type 2",
                    Medications = "Metformin 500mg\nGlipizide 5mg",
                    Instructions = "Take twice daily with meals. Monitor blood glucose levels.",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };
            
            context.PrescriptionTemplates.AddRange(prescriptionTemplates);
            context.SaveChanges();
        }
    }
}