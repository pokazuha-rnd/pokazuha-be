using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pokazuha.Application.Configuration;
using Pokazuha.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokazuha.Infrastructure.Repositories.Implementations
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _sharedFolderPath;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(
            IOptions<FileStorageSettings> fileStorageSettings,
            ILogger<FileStorageService> logger)
        {
            _logger = logger;
            _sharedFolderPath = fileStorageSettings.Value.SharedFolderPath;

            // Validate path
            if (string.IsNullOrWhiteSpace(_sharedFolderPath))
            {
                throw new InvalidOperationException("SharedFolderPath is not configured in appsettings.json");
            }

            // Create SharedFolder if it doesn't exist
            if (!Directory.Exists(_sharedFolderPath))
            {
                Directory.CreateDirectory(_sharedFolderPath);
                _logger.LogInformation($"Created SharedFolder at: {_sharedFolderPath}");
            }
        }

        public async Task<List<string>> SavePostadImagesAsync(Guid postadId, List<IFormFile> images)
        {
            if (images == null || images.Count == 0)
                return new List<string>();

            var savedPaths = new List<string>();

            // Create folder: {SharedFolderPath}/{postadId}/
            var postadFolder = Path.Combine(_sharedFolderPath, postadId.ToString());

            if (!Directory.Exists(postadFolder))
            {
                Directory.CreateDirectory(postadFolder);
            }

            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    try
                    {
                        // Generate unique filename: {guid}_{originalname}
                        var uniqueFileName = $"{Guid.NewGuid()}_{SanitizeFileName(image.FileName)}";
                        var fullPath = Path.Combine(postadFolder, uniqueFileName);

                        // Save file to disk
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        // Return relative path: {postadId}/{filename}
                        var relativePath = Path.Combine(postadId.ToString(), uniqueFileName);
                        savedPaths.Add(relativePath);

                        _logger.LogInformation($"Saved image: {relativePath}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error saving image {image.FileName} for postad {postadId}");
                        throw;
                    }
                }
            }

            return savedPaths;
        }

        public async Task DeleteImageAsync(string filePath)
        {
            try
            {
                var fullPath = GetFullPath(filePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"Deleted image: {filePath}");
                }
                else
                {
                    _logger.LogWarning($"Image not found for deletion: {filePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting image: {filePath}");
                throw;
            }

            await Task.CompletedTask;
        }

        public async Task DeletePostadFolderAsync(Guid postadId)
        {
            try
            {
                var postadFolder = Path.Combine(_sharedFolderPath, postadId.ToString());

                if (Directory.Exists(postadFolder))
                {
                    Directory.Delete(postadFolder, recursive: true);
                    _logger.LogInformation($"Deleted postad folder: {postadId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting postad folder: {postadId}");
                throw;
            }

            await Task.CompletedTask;
        }

        public string GetFullPath(string relativePath)
        {
            // relativePath format: {postadId}/{filename}
            return Path.Combine(_sharedFolderPath, relativePath);
        }

        public bool FileExists(string relativePath)
        {
            var fullPath = GetFullPath(relativePath);
            return File.Exists(fullPath);
        }

        private string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            return sanitized;
        }
    }
}
