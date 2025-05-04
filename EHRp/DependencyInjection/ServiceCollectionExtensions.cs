using System;
using System.IO;
using EHRp.Data;
using EHRp.Data.Repositories;
using EHRp.Services;
using EHRp.ViewModels;
using EHRp.ViewModels.Patients;
using EHRp.ViewModels.Prescriptions;
using EHRp.ViewModels.Visits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace EHRp.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up application services in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all application services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The service collection with added services.</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            // Add logging
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(dispose: true);
            });

            // Add DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Use the project directory for the database during development
                string dbPath = "ehrp.db";
                
                options.UseSqlite($"Data Source={dbPath}");
            }, ServiceLifetime.Scoped); // Explicitly set scoped lifetime

            // Add repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IPatientRepository, PatientRepository>();
            
            // Add services
            services.AddSingleton<IEncryptionKeyManager, EncryptionKeyManager>();
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IMaintenanceService, MaintenanceService>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddSingleton<ThemeManager>();
            
            // Add Navigation Service
            services.AddSingleton<INavigationService, NavigationService>();

            // Add ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton<PatientsViewModel>();
            services.AddSingleton<CalendarViewModel>();
            services.AddSingleton<AppointmentsViewModel>();
            services.AddSingleton<PrescriptionsViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<MaintenanceViewModel>();
            
            // Add new ViewModels for forms
            services.AddTransient<AddPatientViewModel>();
            services.AddTransient<VisitFormViewModel>();
            services.AddTransient<PrescriptionFormViewModel>();

            services.AddSingleton<MainWindowViewModel>();

            return services;
        }
    }
}