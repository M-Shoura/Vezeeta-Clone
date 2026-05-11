using Application.Interfaces.Repositories;
using Domain.Entities.Common;
using Infranstructure.Persistence.Data;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ConcurrentDictionary<Type, object> _repositories = new();
        private bool _disposed;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IGenericRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);
            if (_repositories.TryGetValue(type, out var repository))
            {
                return (IGenericRepository<T>)repository;
            }

            var newRepository = new GenericRepository<T>(_context);
            _repositories[type] = newRepository;

            return newRepository;
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _context.Dispose();
            }

            _disposed = true;
        }
    }
}
