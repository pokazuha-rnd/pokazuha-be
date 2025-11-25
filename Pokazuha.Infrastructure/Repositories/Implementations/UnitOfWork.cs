using Microsoft.EntityFrameworkCore.Storage;
using Pokazuha.Application.Interfaces;
using Pokazuha.Infrastructure.Data;

namespace Pokazuha.Infrastructure.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public IRefreshTokenRepository RefreshTokens { get; }

        public IPostadRepository Postads { get; private set; }
        public IPostadImageRepository PostadImages { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            RefreshTokens = new RefreshTokenRepository(context);
            Postads = new PostadRepository(_context);
            PostadImages = new PostadImageRepository(_context);

        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();

                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
