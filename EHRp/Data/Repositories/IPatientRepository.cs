using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EHRp.Models;

namespace EHRp.Data.Repositories
{
    /// <summary>
    /// Repository interface for patient-related data access operations.
    /// </summary>
    public interface IPatientRepository : IRepository<Patient>
    {
        /// <summary>
        /// Gets patients with their visits asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of patients with their visits.</returns>
        Task<List<Patient>> GetPatientsWithVisitsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a patient with all related data asynchronously.
        /// </summary>
        /// <param name="id">The patient ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The patient with all related data, or null if not found.</returns>
        Task<Patient?> GetPatientWithAllDataAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Searches for patients by name or phone number asynchronously.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of patients matching the search term.</returns>
        Task<List<Patient>> SearchPatientsAsync(string searchTerm, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets patients with upcoming appointments asynchronously.
        /// </summary>
        /// <param name="days">The number of days to look ahead.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of patients with upcoming appointments.</returns>
        Task<List<Patient>> GetPatientsWithUpcomingAppointmentsAsync(int days, CancellationToken cancellationToken = default);
    }
}