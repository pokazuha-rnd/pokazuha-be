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
    public class PostadImageRepository : Repository<PostadImage>, IPostadImageRepository
    {
        public PostadImageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PostadImage>> GetByPostadIdAsync(Guid postadId)
        {
            return await _dbSet
                .Where(i => i.PostadId == postadId)
                .OrderBy(i => i.Order)
                .ToListAsync();
        }

        public async Task<PostadImage?> GetPrimaryImageAsync(Guid postadId)
        {
            return await _dbSet
                .Where(i => i.PostadId == postadId && i.IsPrimary)
                .FirstOrDefaultAsync();
        }

        public async Task SetPrimaryImageAsync(Guid imageId, Guid postadId)
        {
            var images = await _dbSet
                .Where(i => i.PostadId == postadId)
                .ToListAsync();

            foreach (var image in images)
            {
                image.IsPrimary = image.Id == imageId;
                Update(image);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetImageCountForPostadAsync(Guid postadId)
        {
            return await _dbSet
                .CountAsync(i => i.PostadId == postadId);
        }

        public async Task DeleteAllByPostadIdAsync(Guid postadId)
        {
            var images = await _dbSet
                .Where(i => i.PostadId == postadId)
                .ToListAsync();

            DeleteRange(images);
            await _context.SaveChangesAsync();
        }
    }
}
