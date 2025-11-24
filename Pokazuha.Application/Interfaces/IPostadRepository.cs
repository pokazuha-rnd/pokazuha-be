using Pokazuha.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokazuha.Application.Interfaces
{
    public interface IPostadRepository : IRepository<Postad>
    {
        Task<Postad?> GetByIdWithImagesAsync(Guid id);
        Task<Postad?> GetByIdWithUserAsync(Guid id);
        Task<Postad?> GetByIdWithAllDetailsAsync(Guid id);
        Task<IEnumerable<Postad>> GetActivePostadsAsync(int pageNumber, int pageSize);
        Task<IEnumerable<Postad>> GetUserPostadsAsync(string userId, int pageNumber, int pageSize);
        Task<IEnumerable<Postad>> SearchPostadsAsync(string searchTerm, string category, string location,
            decimal? minPrice, decimal? maxPrice, string condition, int pageNumber, int pageSize);
        Task<int> GetTotalActivePostadsCountAsync();
        Task<int> GetUserPostadsCountAsync(string userId);
        Task<int> GetSearchResultsCountAsync(string searchTerm, string category, string location,
            decimal? minPrice, decimal? maxPrice, string condition);
        Task<IEnumerable<Postad>> GetPendingPostadsAsync();
        Task IncrementViewCountAsync(Guid id);
        Task<bool> IsUserOwnerAsync(Guid postadId, string userId);
    }
}
