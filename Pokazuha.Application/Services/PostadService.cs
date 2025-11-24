using AutoMapper;
using Microsoft.Extensions.Logging;
using Pokazuha.Application.DTOs.Postad;
using Pokazuha.Application.Interfaces;
using Pokazuha.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokazuha.Application.Services
{
    public class PostadService : IPostadService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMapper _mapper;
        private readonly ILogger<PostadService> _logger;

        public PostadService(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            IMapper mapper,
            ILogger<PostadService> logger)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PostadDto> CreatePostadAsync(CreatePostadRequestDto request, string userId)
        {
            try
            {
                var postad = _mapper.Map<Postad>(request);
                postad.Id = Guid.NewGuid();
                postad.UserId = userId;
                postad.Status = "Active"; // Change after approval created
                postad.CreatedAt = DateTime.UtcNow;
                postad.UpdatedAt = DateTime.UtcNow;

 
                await _unitOfWork.Postads.AddAsync(postad);
                await _unitOfWork.SaveChangesAsync();

                if (request.Images != null && request.Images.Count > 0)
                {
                    var imagePaths = await _fileStorageService.SavePostadImagesAsync(postad.Id, request.Images);

                    for (int i = 0; i < imagePaths.Count; i++)
                    {
                        var postadImage = new PostadImage
                        {
                            Id = Guid.NewGuid(),
                            PostadId = postad.Id,
                            ImageUrl = imagePaths[i],
                            FileName = request.Images[i].FileName,
                            FileSize = request.Images[i].Length,
                            IsPrimary = i == 0,
                            Order = i,
                            UploadedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.PostadImages.AddAsync(postadImage);
                    }

                    await _unitOfWork.SaveChangesAsync();
                }

                var createdPostad = await _unitOfWork.Postads.GetByIdWithAllDetailsAsync(postad.Id);

                return _mapper.Map<PostadDto>(createdPostad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating postad for user {userId}");
                throw;
            }
        }

        public async Task<PostadDto> GetPostadByIdAsync(Guid id)
        {
            var postad = await _unitOfWork.Postads.GetByIdWithAllDetailsAsync(id);

            if (postad == null)
            {
                throw new KeyNotFoundException($"Postad with ID {id} not found");
            }

            return _mapper.Map<PostadDto>(postad);
        }

        public async Task<PaginatedPostadsDto> GetActivePostadsAsync(int pageNumber = 1, int pageSize = 20)
        {
            var postads = await _unitOfWork.Postads.GetActivePostadsAsync(pageNumber, pageSize);
            var totalCount = await _unitOfWork.Postads.GetTotalActivePostadsCountAsync();

            return new PaginatedPostadsDto
            {
                Postads = _mapper.Map<List<PostadListDto>>(postads),
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<PaginatedPostadsDto> GetUserPostadsAsync(string userId, int pageNumber = 1, int pageSize = 20)
        {
            var postads = await _unitOfWork.Postads.GetUserPostadsAsync(userId, pageNumber, pageSize);
            var totalCount = await _unitOfWork.Postads.GetUserPostadsCountAsync(userId);

            return new PaginatedPostadsDto
            {
                Postads = _mapper.Map<List<PostadListDto>>(postads),
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<PaginatedPostadsDto> SearchPostadsAsync(
            string searchTerm,
            string category,
            string location,
            decimal? minPrice,
            decimal? maxPrice,
            string condition,
            int pageNumber = 1,
            int pageSize = 20)
        {
            var postads = await _unitOfWork.Postads.SearchPostadsAsync(
                searchTerm, category, location, minPrice, maxPrice, condition, pageNumber, pageSize);

            var totalCount = await _unitOfWork.Postads.GetSearchResultsCountAsync(
                searchTerm, category, location, minPrice, maxPrice, condition);

            return new PaginatedPostadsDto
            {
                Postads = _mapper.Map<List<PostadListDto>>(postads),
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<PostadDto> UpdatePostadAsync(UpdatePostadRequestDto request, string userId)
        {
            try
            {
                var userGuid = Guid.Parse(userId);

                // Verify ownership
                var isOwner = await _unitOfWork.Postads.IsUserOwnerAsync(request.Id, userId);
                if (!isOwner)
                {
                    throw new UnauthorizedAccessException("You don't have permission to update this postad");
                }

                // Get existing postad
                var postad = await _unitOfWork.Postads.GetByIdWithImagesAsync(request.Id);
                if (postad == null)
                {
                    throw new KeyNotFoundException($"Postad with ID {request.Id} not found");
                }

                // Update properties
                postad.Title = request.Title;
                postad.Description = request.Description;
                postad.Price = request.Price;
                postad.Currency = request.Currency;
                postad.Category = request.Category;
                postad.Condition = request.Condition;
                postad.Location = request.Location;
                postad.PhoneNumber = request.PhoneNumber;
                postad.ShowEmailToPublic = request.ShowEmailToPublic;
                postad.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Postads.Update(postad);

                // Handle image deletions
                if (request.ImageIdsToDelete != null && request.ImageIdsToDelete.Count > 0)
                {
                    foreach (var imageId in request.ImageIdsToDelete)
                    {
                        var image = await _unitOfWork.PostadImages.GetByIdAsync(imageId);
                        if (image != null && image.PostadId == request.Id)
                        {
                            await _fileStorageService.DeleteImageAsync(image.ImageUrl);
                            _unitOfWork.PostadImages.Delete(image);
                        }
                    }
                }

                // Handle new images
                if (request.NewImages != null && request.NewImages.Count > 0)
                {
                    var currentImageCount = await _unitOfWork.PostadImages.GetImageCountForPostadAsync(request.Id);

                    if (currentImageCount + request.NewImages.Count > 10)
                    {
                        throw new InvalidOperationException("Cannot exceed 10 images per postad");
                    }

                    var imagePaths = await _fileStorageService.SavePostadImagesAsync(postad.Id, request.NewImages);

                    for (int i = 0; i < imagePaths.Count; i++)
                    {
                        var postadImage = new PostadImage
                        {
                            Id = Guid.NewGuid(),
                            PostadId = postad.Id,
                            ImageUrl = imagePaths[i],
                            FileName = request.NewImages[i].FileName,
                            FileSize = request.NewImages[i].Length,
                            IsPrimary = currentImageCount == 0 && i == 0,
                            Order = currentImageCount + i,
                            UploadedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.PostadImages.AddAsync(postadImage);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                // Reload with relationships
                var updatedPostad = await _unitOfWork.Postads.GetByIdWithAllDetailsAsync(postad.Id);

                return _mapper.Map<PostadDto>(updatedPostad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating postad {request.Id}");
                throw;
            }
        }

        public async Task<bool> DeletePostadAsync(Guid id, string userId)
        {
            try
            {
                var userGuid = Guid.Parse(userId);

                // Verify ownership
                var isOwner = await _unitOfWork.Postads.IsUserOwnerAsync(id, userId);
                if (!isOwner)
                {
                    throw new UnauthorizedAccessException("You don't have permission to delete this postad");
                }

                var postad = await _unitOfWork.Postads.GetByIdAsync(id);
                if (postad == null)
                {
                    throw new KeyNotFoundException($"Postad with ID {id} not found");
                }

                // Delete all images from file system
                await _fileStorageService.DeletePostadFolderAsync(id);

                // Soft delete (set status to Deleted)
                postad.Status = "Deleted";
                postad.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Postads.Update(postad);

                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting postad {id}");
                throw;
            }
        }

        public async Task<PostadDto> ApprovePostadAsync(Guid id, string adminUserId)
        {
            var postad = await _unitOfWork.Postads.GetByIdWithAllDetailsAsync(id);

            if (postad == null)
            {
                throw new KeyNotFoundException($"Postad with ID {id} not found");
            }

            postad.Status = "Active";
            postad.ApprovedAt = DateTime.UtcNow;
            postad.ApprovedByUserId = Guid.Parse(adminUserId);
            postad.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Postads.Update(postad);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PostadDto>(postad);
        }

        public async Task<PostadDto> RejectPostadAsync(Guid id, string adminUserId)
        {
            var postad = await _unitOfWork.Postads.GetByIdWithAllDetailsAsync(id);

            if (postad == null)
            {
                throw new KeyNotFoundException($"Postad with ID {id} not found");
            }

            postad.Status = "Rejected";
            postad.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Postads.Update(postad);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PostadDto>(postad);
        }

        public async Task IncrementViewCountAsync(Guid id)
        {
            await _unitOfWork.Postads.IncrementViewCountAsync(id);
        }
    }
}
