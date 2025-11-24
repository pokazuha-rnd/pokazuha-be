using Pokazuha.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokazuha.Application.Interfaces
{
    public interface IPostadImageRepository : IRepository<PostadImage>
    {
        Task<IEnumerable<PostadImage>> GetByPostadIdAsync(Guid postadId);
        Task<PostadImage?> GetPrimaryImageAsync(Guid postadId);
        Task SetPrimaryImageAsync(Guid imageId, Guid postadId);
        Task<int> GetImageCountForPostadAsync(Guid postadId);
        Task DeleteAllByPostadIdAsync(Guid postadId);
    }
}
