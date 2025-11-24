using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokazuha.Application.Interfaces
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves uploaded images to SharedFolder/{postadId}/
        /// </summary>
        /// <returns>List of relative file paths</returns>
        Task<List<string>> SavePostadImagesAsync(Guid postadId, List<IFormFile> images);

        /// <summary>
        /// Deletes a single image file
        /// </summary>
        Task DeleteImageAsync(string filePath);

        /// <summary>
        /// Deletes entire postad folder with all images
        /// </summary>
        Task DeletePostadFolderAsync(Guid postadId);

        /// <summary>
        /// Gets full physical path for a relative path
        /// </summary>
        string GetFullPath(string relativePath);

        /// <summary>
        /// Checks if file exists
        /// </summary>
        bool FileExists(string relativePath);
    }
}
