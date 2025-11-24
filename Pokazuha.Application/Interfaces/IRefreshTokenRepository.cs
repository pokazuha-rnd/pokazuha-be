using Pokazuha.Domain.Entities;

namespace Pokazuha.Application.Interfaces
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);
        Task RevokeAllUserTokensAsync(string userId);
        Task RevokeTokenAsync(string token, string revokedByIp);
        Task<int> DeleteExpiredTokensAsync();
    }
}
