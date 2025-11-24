using Microsoft.EntityFrameworkCore;
using Pokazuha.Application.Interfaces;
using Pokazuha.Domain.Entities;
using Pokazuha.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokazuha.Infrastructure.Repositories.Implementations
{
    public class PostadRepository : Repository<Postad>, IPostadRepository
    {
        public PostadRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Postad?> GetByIdWithImagesAsync(Guid id)
        {
            return await _dbSet
                .Include(p => p.Images.OrderBy(i => i.Order))
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Postad?> GetByIdWithUserAsync(Guid id)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Postad?> GetByIdWithAllDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(p => p.Images.OrderBy(i => i.Order))
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Postad>> GetActivePostadsAsync(int pageNumber, int pageSize)
        {
            return await _dbSet
                .Where(p => p.Status == "Active")
                .Include(p => p.Images.Where(i => i.IsPrimary).Take(1))
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Postad>> GetUserPostadsAsync(string userId, int pageNumber, int pageSize)
        {
            return await _dbSet
                .Where(p => p.UserId == userId)
                .Include(p => p.Images.Where(i => i.IsPrimary).Take(1))
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Postad>> SearchPostadsAsync(
            string searchTerm,
            string category,
            string location,
            decimal? minPrice,
            decimal? maxPrice,
            string condition,
            int pageNumber,
            int pageSize)
        {
            var query = _dbSet
                .Where(p => p.Status == "Active")
                .Include(p => p.Images.Where(i => i.IsPrimary).Take(1))
                .Include(p => p.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Title.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(p => p.Location.Contains(location));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(condition))
            {
                query = query.Where(p => p.Condition == condition);
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalActivePostadsCountAsync()
        {
            return await _dbSet
                .CountAsync(p => p.Status == "Active");
        }

        public async Task<int> GetUserPostadsCountAsync(string userId)
        {
            return await _dbSet
                .CountAsync(p => p.UserId == userId);
        }

        public async Task<int> GetSearchResultsCountAsync(
            string searchTerm,
            string category,
            string location,
            decimal? minPrice,
            decimal? maxPrice,
            string condition)
        {
            var query = _dbSet
                .Where(p => p.Status == "Active")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Title.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(p => p.Location.Contains(location));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(condition))
            {
                query = query.Where(p => p.Condition == condition);
            }

            return await query.CountAsync();
        }

        public async Task<IEnumerable<Postad>> GetPendingPostadsAsync()
        {
            return await _dbSet
                .Where(p => p.Status == "Pending")
                .Include(p => p.Images.Where(i => i.IsPrimary).Take(1))
                .Include(p => p.User)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task IncrementViewCountAsync(Guid id)
        {
            var postad = await GetByIdAsync(id);
            if (postad != null)
            {
                postad.ViewCount++;
                Update(postad);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsUserOwnerAsync(Guid postadId, string userId) 
        {
            return await _dbSet
                .AnyAsync(p => p.Id == postadId && p.UserId == userId);
        }
    }
}
