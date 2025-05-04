using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EHRp.Data.Repositories
{
    /// <summary>
    /// Generic repository implementation for data access operations.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger _logger;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{T}"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public Repository(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbSet = _context.Set<T>();
        }
        
        /// <inheritdoc/>
        public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all entities of type {EntityType}", typeof(T).Name);
                throw new InvalidOperationException($"Failed to retrieve entities of type {typeof(T).Name}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public virtual async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding entities of type {EntityType} with predicate", typeof(T).Name);
                throw new InvalidOperationException($"Failed to find entities of type {typeof(T).Name}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public virtual async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.FindAsync(new[] { id }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
                throw new InvalidOperationException($"Failed to retrieve entity of type {typeof(T).Name} with ID {id}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            
            try
            {
                await _dbSet.AddAsync(entity, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding entity of type {EntityType}", typeof(T).Name);
                throw new InvalidOperationException($"Failed to add entity of type {typeof(T).Name}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            
            try
            {
                _dbSet.Update(entity);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity of type {EntityType}", typeof(T).Name);
                throw new InvalidOperationException($"Failed to update entity of type {typeof(T).Name}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public virtual async Task DeleteAsync(object id, CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await GetByIdAsync(id, cancellationToken);
                if (entity != null)
                {
                    _dbSet.Remove(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
                throw new InvalidOperationException($"Failed to delete entity of type {typeof(T).Name} with ID {id}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.CountAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting entities of type {EntityType}", typeof(T).Name);
                throw new InvalidOperationException($"Failed to count entities of type {typeof(T).Name}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.CountAsync(predicate, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting entities of type {EntityType} with predicate", typeof(T).Name);
                throw new InvalidOperationException($"Failed to count entities of type {typeof(T).Name}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.AnyAsync(predicate, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if any entity of type {EntityType} matches predicate", typeof(T).Name);
                throw new InvalidOperationException($"Failed to check if any entity of type {typeof(T).Name} matches predicate.", ex);
            }
        }
        
        /// <inheritdoc/>
        public virtual async Task<(List<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than or equal to 1.");
            }
            
            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than or equal to 1.");
            }
            
            try
            {
                var totalCount = await CountAsync(cancellationToken);
                
                var items = await _dbSet.AsNoTracking()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);
                
                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged entities of type {EntityType}. Page: {PageNumber}, Size: {PageSize}", 
                    typeof(T).Name, pageNumber, pageSize);
                throw new InvalidOperationException($"Failed to retrieve paged entities of type {typeof(T).Name}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public virtual async Task<(List<T> Items, int TotalCount)> GetPagedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than or equal to 1.");
            }
            
            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than or equal to 1.");
            }
            
            try
            {
                var totalCount = await CountAsync(predicate, cancellationToken);
                
                var items = await _dbSet.AsNoTracking()
                    .Where(predicate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);
                
                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged entities of type {EntityType} with predicate. Page: {PageNumber}, Size: {PageSize}", 
                    typeof(T).Name, pageNumber, pageSize);
                throw new InvalidOperationException($"Failed to retrieve paged entities of type {typeof(T).Name}.", ex);
            }
        }
    }
}