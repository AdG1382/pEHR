using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EHRp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EHRp.Data.Repositories
{
    /// <summary>
    /// Repository implementation for patient-related data access operations.
    /// </summary>
    public class PatientRepository : Repository<Patient>, IPatientRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PatientRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public PatientRepository(ApplicationDbContext context, ILogger<PatientRepository> logger)
            : base(context, logger)
        {
        }
        
        /// <inheritdoc/>
        public async Task<List<Patient>> GetPatientsWithVisitsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Patients
                    .AsNoTracking()
                    .Include(p => p.Visits)
                    .OrderBy(p => p.LastName)
                    .ThenBy(p => p.FirstName)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patients with visits");
                throw new InvalidOperationException("Failed to retrieve patients with visits.", ex);
            }
        }
        
        /// <inheritdoc/>
        public async Task<Patient?> GetPatientWithAllDataAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Patients
                    .Include(p => p.Visits)
                    .Include(p => p.Prescriptions)
                    .Include(p => p.Files)
                    .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patient with all data. Patient ID: {PatientId}", id);
                throw new InvalidOperationException($"Failed to retrieve patient with ID {id} and all related data.", ex);
            }
        }
        
        /// <inheritdoc/>
        public async Task<List<Patient>> SearchPatientsAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllAsync(cancellationToken);
            }
            
            try
            {
                searchTerm = searchTerm.ToLower();
                
                return await _context.Patients
                    .AsNoTracking()
                    .Where(p => 
                        p.FirstName.ToLower().Contains(searchTerm) || 
                        p.LastName.ToLower().Contains(searchTerm) || 
                        p.PhoneNumber.Contains(searchTerm))
                    .OrderBy(p => p.LastName)
                    .ThenBy(p => p.FirstName)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching patients with term: {SearchTerm}", searchTerm);
                throw new InvalidOperationException($"Failed to search patients with term: {searchTerm}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public async Task<List<Patient>> GetPatientsWithUpcomingAppointmentsAsync(int days, CancellationToken cancellationToken = default)
        {
            try
            {
                var endDate = DateTime.Now.AddDays(days);
                
                // Get appointments in the date range
                var appointmentsInRange = await _context.Appointments
                    .AsNoTracking()
                    .Where(a => a.AppointmentDate >= DateTime.Now && a.AppointmentDate <= endDate)
                    .OrderBy(a => a.AppointmentDate)
                    .Select(a => a.PatientId)
                    .Distinct()
                    .ToListAsync(cancellationToken);
                
                // Get the patients with those appointments
                return await _context.Patients
                    .AsNoTracking()
                    .Where(p => appointmentsInRange.Contains(p.Id))
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patients with upcoming appointments for the next {Days} days", days);
                throw new InvalidOperationException($"Failed to retrieve patients with upcoming appointments for the next {days} days.", ex);
            }
        }
    }
}