using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EHRp.Data.Repositories
{
    /// <summary>
    /// Generic repository interface for data access operations.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets all entities asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of all entities.</returns>
        Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets entities that match the specified predicate asynchronously.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of entities that match the predicate.</returns>
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets an entity by ID asynchronously.
        /// </summary>
        /// <param name="id">The entity ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The entity with the specified ID, or null if not found.</returns>
        Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a new entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The added entity.</returns>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deletes an entity by ID asynchronously.
        /// </summary>
        /// <param name="id">The entity ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAsync(object id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the count of entities asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The count of entities.</returns>
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the count of entities that match the specified predicate asynchronously.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The count of entities that match the predicate.</returns>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if any entity matches the specified predicate asynchronously.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if any entity matches the predicate, false otherwise.</returns>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets entities with pagination asynchronously.
        /// </summary>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A tuple containing the entities for the requested page and the total count of entities.</returns>
        Task<(List<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets entities that match the specified predicate with pagination asynchronously.
        /// </summary>
        /// <param name="predicate">The predicate to filter entities.</param>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A tuple containing the entities for the requested page and the total count of entities that match the predicate.</returns>
        Task<(List<T> Items, int TotalCount)> GetPagedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }
}