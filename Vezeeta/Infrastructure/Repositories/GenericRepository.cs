using Domain.Entities.Common;
using Infranstructure.Persistence.Data;
using Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        // ==================== CREATE ====================

        /// <summary>
        /// Add a single entity to the database
        /// </summary>
        public async Task<T> AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity);
            return entity;
        }

        // ==================== READ ====================

        /// <summary>
        /// Get entity by Id (returns null if not found)
        /// </summary>
        public async Task<T?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id must be greater than 0", nameof(id));

            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        /// <summary>
        /// Get all entities (soft-deleted entities are excluded automatically via query filter)
        /// </summary>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// General query method for filtering entities with Lambda expression
        /// </summary>
        public async Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
        }

        // ==================== UPDATE ====================

        /// <summary>
        /// Update an existing entity
        /// </summary>
        public async Task UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // If it's an AuditableEntity, update the UpdatedAt timestamp
            if (entity is AuditableEntity auditableEntity)
            {
                auditableEntity.UpdatedAt = DateTime.UtcNow;
            }

            _dbSet.Update(entity);
        }

        // ==================== DELETE ====================

        /// <summary>
        /// Delete entity by Id (hard delete for BaseEntity, soft delete for AuditableEntity)
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id must be greater than 0", nameof(id));

            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                // If it's an AuditableEntity, do soft delete
                if (entity is AuditableEntity auditableEntity)
                {
                    auditableEntity.IsDeleted = true;
                    auditableEntity.UpdatedAt = DateTime.UtcNow;
                    _dbSet.Update(entity);
                }
                else
                {
                    // Otherwise, hard delete
                    _dbSet.Remove(entity);
                }
            }
        }

        // ==================== SAVE ====================

        /// <summary>
        /// Save all changes to the database
        /// </summary>
        public async Task<bool> SaveChangesAsync()
        {
            try
            {
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("An error occurred while saving changes to the database.", ex);
            }
        }
    }
}
