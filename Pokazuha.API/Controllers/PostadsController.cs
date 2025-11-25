using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pokazuha.Application.DTOs.Postad;
using Pokazuha.Application.Interfaces;
using System.Security.Claims;

namespace Pokazuha.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostadsController : ControllerBase
    {
        private readonly IPostadService _postadService;
        private readonly ILogger<PostadsController> _logger;

        public PostadsController(
            IPostadService postadService,
            ILogger<PostadsController> logger)
        {
            _postadService = postadService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active postads with pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedPostadsDto>> GetActivePostads(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _postadService.GetActivePostadsAsync(pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active postads");
                return StatusCode(500, new { message = "An error occurred while retrieving postads" });
            }
        }

        /// <summary>
        /// Search postads with filters
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedPostadsDto>> SearchPostads(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? category = null,
            [FromQuery] string? location = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? condition = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _postadService.SearchPostadsAsync(
                    searchTerm, category, location, minPrice, maxPrice, condition, pageNumber, pageSize);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching postads");
                return StatusCode(500, new { message = "An error occurred while searching postads" });
            }
        }

        /// <summary>
        /// Get postad by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PostadDto>> GetPostadById(Guid id)
        {
            try
            {
                var postad = await _postadService.GetPostadByIdAsync(id);

                // Increment view count (fire and forget)
                _ = _postadService.IncrementViewCountAsync(id);

                return Ok(postad);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting postad {id}");
                return StatusCode(500, new { message = "An error occurred while retrieving the postad" });
            }
        }

        /// <summary>
        /// Get current user's postads
        /// </summary>
        [HttpGet("my-postads")]
        [Authorize]
        public async Task<ActionResult<PaginatedPostadsDto>> GetMyPostads(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _postadService.GetUserPostadsAsync(userId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user postads");
                return StatusCode(500, new { message = "An error occurred while retrieving your postads" });
            }
        }

        /// <summary>
        /// Create new postad
        /// </summary>
        [HttpPost]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<PostadDto>> CreatePostad([FromForm] CreatePostadRequestDto request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var postad = await _postadService.CreatePostadAsync(request, userId);

                return CreatedAtAction(
                    nameof(GetPostadById),
                    new { id = postad.Id },
                    postad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating postad");
                return StatusCode(500, new { message = "An error occurred while creating the postad" });
            }
        }

        /// <summary>
        /// Update existing postad
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<PostadDto>> UpdatePostad(Guid id, [FromForm] UpdatePostadRequestDto request)
        {
            try
            {
                if (id != request.Id)
                {
                    return BadRequest(new { message = "ID mismatch" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var postad = await _postadService.UpdatePostadAsync(request, userId);
                return Ok(postad);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating postad {id}");
                return StatusCode(500, new { message = "An error occurred while updating the postad" });
            }
        }

        /// <summary>
        /// Delete postad
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeletePostad(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _postadService.DeletePostadAsync(id, userId);

                if (result)
                {
                    return NoContent();
                }

                return NotFound(new { message = "Postad not found" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting postad {id}");
                return StatusCode(500, new { message = "An error occurred while deleting the postad" });
            }
        }

        /// <summary>
        /// Approve postad (Admin only)
        /// </summary>
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PostadDto>> ApprovePostad(Guid id)
        {
            try
            {
                var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(adminUserId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var postad = await _postadService.ApprovePostadAsync(id, adminUserId);
                return Ok(postad);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving postad {id}");
                return StatusCode(500, new { message = "An error occurred while approving the postad" });
            }
        }

        /// <summary>
        /// Reject postad (Admin only)
        /// </summary>
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PostadDto>> RejectPostad(Guid id)
        {
            try
            {
                var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(adminUserId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var postad = await _postadService.RejectPostadAsync(id, adminUserId);
                return Ok(postad);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rejecting postad {id}");
                return StatusCode(500, new { message = "An error occurred while rejecting the postad" });
            }
        }
    }
}
