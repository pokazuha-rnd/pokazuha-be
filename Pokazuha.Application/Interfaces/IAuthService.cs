using Pokazuha.Application.DTOs.Auth;
using Pokazuha.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokazuha.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto, string ipAddress);
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto, string ipAddress);
        Task<ApiResponse<AuthResponseDto>> GoogleLoginAsync(GoogleLoginDto dto, string ipAddress);
        Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string token, string ipAddress);
        Task<ApiResponse<bool>> RevokeTokenAsync(string token, string ipAddress);
        Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto dto);
        Task<ApiResponse<UserDto>> GetUserByIdAsync(string userId);
        Task<ApiResponse<UserDto>> GetCurrentUserAsync(string userId);
    }
}
