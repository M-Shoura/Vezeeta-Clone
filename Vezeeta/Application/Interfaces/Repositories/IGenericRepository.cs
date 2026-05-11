using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        // CREATE
        Task<T> AddAsync(T entity);

        // READ
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate);

        // UPDATE
        Task UpdateAsync(T entity);

        // DELETE
        Task DeleteAsync(int id);

        // SAVE
        Task<bool> SaveChangesAsync();
    }
}
