using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using EHRp.Data.Repositories;
using EHRp.Messages;
using EHRp.Models;
using Microsoft.Extensions.Logging;

namespace EHRp.Services
{
    /// <summary>
    /// Interface for patient-related services.
    /// </summary>
    public interface IPatientService
    {
        /// <summary>
        /// Gets all patients asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of patients.</returns>
        Task<List<Patient>> GetAllPatientsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a patient by ID asynchronously.
        /// </summary>
        /// <param name="id">The patient ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The patient, or null if not found.</returns>
        Task<Patient?> GetPatientByIdAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a patient with all related data asynchronously.
        /// </summary>
        /// <param name="id">The patient ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The patient with all related data, or null if not found.</returns>
        Task<Patient?> GetPatientWithAllDataAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Searches for patients asynchronously.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of patients matching the search term.</returns>
        Task<List<Patient>> SearchPatientsAsync(string searchTerm, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a new patient asynchronously.
        /// </summary>
        /// <param name="patient">The patient to add.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The added patient with ID assigned.</returns>
        Task<Patient> AddPatientAsync(Patient patient, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates a patient asynchronously.
        /// </summary>
        /// <param name="patient">The patient to update.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the patient was updated, false otherwise.</returns>
        Task<bool> UpdatePatientAsync(Patient patient, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deletes a patient asynchronously.
        /// </summary>
        /// <param name="id">The patient ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the patient was deleted, false otherwise.</returns>
        Task<bool> DeletePatientAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets patients with pagination asynchronously.
        /// </summary>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A tuple containing the patients for the requested page and the total count of patients.</returns>
        Task<(List<Patient> Patients, int TotalCount)> GetPatientsPaginatedAsync(
            int pageNumber, 
            int pageSize, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets patients with upcoming appointments asynchronously.
        /// </summary>
        /// <param name="days">The number of days to look ahead.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of patients with upcoming appointments.</returns>
        Task<List<Patient>> GetPatientsWithUpcomingAppointmentsAsync(int days, CancellationToken cancellationToken = default);
    }
    
    /// <summary>
    /// Implementation of the patient service.
    /// </summary>
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ILogger<PatientService> _logger;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PatientService"/> class.
        /// </summary>
        /// <param name="patientRepository">The patient repository.</param>
        /// <param name="logger">The logger instance.</param>
        public PatientService(IPatientRepository patientRepository, ILogger<PatientService> logger)
        {
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <inheritdoc/>
        public async Task<List<Patient>> GetAllPatientsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _patientRepository.GetAllAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all patients");
                throw new InvalidOperationException("Failed to retrieve patients.", ex);
            }
        }
        
        /// <inheritdoc/>
        public async Task<Patient?> GetPatientByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _patientRepository.GetByIdAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patient by ID: {PatientId}", id);
                throw new InvalidOperationException($"Failed to retrieve patient with ID {id}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public async Task<Patient?> GetPatientWithAllDataAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _patientRepository.GetPatientWithAllDataAsync(id, cancellationToken);
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
            try
            {
                return await _patientRepository.SearchPatientsAsync(searchTerm, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching patients with term: {SearchTerm}", searchTerm);
                throw new InvalidOperationException($"Failed to search patients with term: {searchTerm}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public async Task<Patient> AddPatientAsync(Patient patient, CancellationToken cancellationToken = default)
        {
            if (patient == null)
            {
                throw new ArgumentNullException(nameof(patient));
            }
            
            try
            {
                // Set created date
                patient.CreatedAt = DateTime.Now;
                patient.UpdatedAt = DateTime.Now;
                
                var addedPatient = await _patientRepository.AddAsync(patient, cancellationToken);
                
                _logger.LogInformation("Added new patient: {PatientId} - {PatientName}", 
                    addedPatient.Id, $"{addedPatient.FirstName} {addedPatient.LastName}");
                
                // Send message that patient was added
                WeakReferenceMessenger.Default.Send(new PatientUpdatedMessage(addedPatient, true));
                
                return addedPatient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding patient: {PatientName}", 
                    $"{patient.FirstName} {patient.LastName}");
                throw new InvalidOperationException("Failed to add patient.", ex);
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> UpdatePatientAsync(Patient patient, CancellationToken cancellationToken = default)
        {
            if (patient == null)
            {
                throw new ArgumentNullException(nameof(patient));
            }
            
            try
            {
                // Check if patient exists
                var existingPatient = await _patientRepository.GetByIdAsync(patient.Id, cancellationToken);
                if (existingPatient == null)
                {
                    _logger.LogWarning("Patient not found for update: {PatientId}", patient.Id);
                    return false;
                }
                
                // Update timestamp
                patient.UpdatedAt = DateTime.Now;
                patient.CreatedAt = existingPatient.CreatedAt; // Preserve original creation date
                
                await _patientRepository.UpdateAsync(patient, cancellationToken);
                
                _logger.LogInformation("Updated patient: {PatientId} - {PatientName}", 
                    patient.Id, $"{patient.FirstName} {patient.LastName}");
                
                // Send message that patient was updated
                WeakReferenceMessenger.Default.Send(new PatientUpdatedMessage(patient, false));
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient: {PatientId}", patient.Id);
                throw new InvalidOperationException($"Failed to update patient with ID {patient.Id}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> DeletePatientAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if patient exists
                var patient = await _patientRepository.GetByIdAsync(id, cancellationToken);
                if (patient == null)
                {
                    _logger.LogWarning("Patient not found for deletion: {PatientId}", id);
                    return false;
                }
                
                await _patientRepository.DeleteAsync(id, cancellationToken);
                
                _logger.LogInformation("Deleted patient: {PatientId}", id);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient: {PatientId}", id);
                throw new InvalidOperationException($"Failed to delete patient with ID {id}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public async Task<(List<Patient> Patients, int TotalCount)> GetPatientsPaginatedAsync(
            int pageNumber, 
            int pageSize, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _patientRepository.GetPagedAsync(pageNumber, pageSize, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated patients. Page: {PageNumber}, Size: {PageSize}", 
                    pageNumber, pageSize);
                throw new InvalidOperationException("Failed to retrieve paginated patients.", ex);
            }
        }
        
        /// <inheritdoc/>
        public async Task<List<Patient>> GetPatientsWithUpcomingAppointmentsAsync(int days, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _patientRepository.GetPatientsWithUpcomingAppointmentsAsync(days, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patients with upcoming appointments for the next {Days} days", days);
                throw new InvalidOperationException($"Failed to retrieve patients with upcoming appointments for the next {days} days.", ex);
            }
        }
    }
}