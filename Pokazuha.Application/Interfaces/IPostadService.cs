using Pokazuha.Application.DTOs.Postad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokazuha.Application.Interfaces
{
    public interface IPostadService
    {
        // Create
        Task<PostadDto> CreatePostadAsync(CreatePostadRequestDto request, string userId);

        // Read
        Task<PostadDto> GetPostadByIdAsync(Guid id);
        Task<PaginatedPostadsDto> GetActivePostadsAsync(int pageNumber = 1, int pageSize = 20);
        Task<PaginatedPostadsDto> GetUserPostadsAsync(string userId, int pageNumber = 1, int pageSize = 20);
        Task<PaginatedPostadsDto> SearchPostadsAsync(
            string searchTerm,
            string category,
            string location,
            decimal? minPrice,
            decimal? maxPrice,
            string condition,
            int pageNumber = 1,
            int pageSize = 20);

        // Update
        Task<PostadDto> UpdatePostadAsync(UpdatePostadRequestDto request, string userId);

        // Delete
        Task<bool> DeletePostadAsync(Guid id, string userId);

        // Admin
        Task<PostadDto> ApprovePostadAsync(Guid id, string adminUserId);
        Task<PostadDto> RejectPostadAsync(Guid id, string adminUserId);

        // Other
        Task IncrementViewCountAsync(Guid id);
    }
}
