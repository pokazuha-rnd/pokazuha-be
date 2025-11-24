using Microsoft.EntityFrameworkCore;
using Pokazuha.Application.Interfaces;
using Pokazuha.Domain.Entities;
using Pokazuha.Infrastructure.Data;

namespace Pokazuha.Infrastructure.Repositories.Implementations
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _dbSet
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
        {
            return await _dbSet
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task RevokeAllUserTokensAsync(string userId)
        {
            var tokens = await _dbSet
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }
        }

        public async Task RevokeTokenAsync(string token, string revokedByIp)
        {
            var refreshToken = await GetByTokenAsync(token);

            if (refreshToken != null && refreshToken.IsActive)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
                refreshToken.RevokedByIp = revokedByIp;
            }
        }

        public async Task<int> DeleteExpiredTokensAsync()
        {
            var expiredTokens = await _dbSet
                .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            _dbSet.RemoveRange(expiredTokens);

            return expiredTokens.Count;
        }
    }
}
