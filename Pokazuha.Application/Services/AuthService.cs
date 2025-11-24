using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Pokazuha.Application.DTOs.Auth;
using Pokazuha.Application.DTOs.Common;
using Pokazuha.Application.Interfaces;
using Pokazuha.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Pokazuha.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IGoogleAuthService googleAuthService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _googleAuthService = googleAuthService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto, string ipAddress)
        {
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return ApiResponse<AuthResponseDto>.FailureResponse(
                    "Registration failed",
                    new List<string> { "Email is already registered" });
            }

            // Create new user
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                return ApiResponse<AuthResponseDto>.FailureResponse(
                    "Registration failed",
                    result.Errors.Select(e => e.Description).ToList());
            }

            // Assign default "User" role
            await _userManager.AddToRoleAsync(user, "User");

            // Generate tokens
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken(ipAddress);

            // Save refresh token
            refreshToken.UserId = user.Id;
            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles.ToList();

            var authResponse = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = userDto
            };

            return ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "Registration successful");
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto, string ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !user.IsActive)
            {
                return ApiResponse<AuthResponseDto>.FailureResponse(
                    "Login failed",
                    new List<string> { "Invalid email or password" });
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!isPasswordValid)
            {
                return ApiResponse<AuthResponseDto>.FailureResponse(
                    "Login failed",
                    new List<string> { "Invalid email or password" });
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Generate tokens
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken(ipAddress);

            // Revoke old refresh tokens if not RememberMe
            if (!dto.RememberMe)
            {
                await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(user.Id);
            }

            // Save new refresh token
            refreshToken.UserId = user.Id;
            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles.ToList();

            var authResponse = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = userDto
            };

            return ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "Login successful");
        }

        public async Task<ApiResponse<AuthResponseDto>> GoogleLoginAsync(GoogleLoginDto dto, string ipAddress)
        {
            // Validate Google token
            var googleUser = await _googleAuthService.ValidateGoogleTokenAsync(dto.IdToken);

            if (googleUser == null)
            {
                return ApiResponse<AuthResponseDto>.FailureResponse(
                    "Google authentication failed",
                    new List<string> { "Invalid Google token" });
            }

            // Check if user exists by GoogleId
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.GoogleId == googleUser.GoogleId);

            // If not found by GoogleId, check by email
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(googleUser.Email);

                if (user != null)
                {
                    // Link Google account to existing user
                    user.GoogleId = googleUser.GoogleId;
                    user.IsGoogleUser = true;
                    user.IsVerified = googleUser.EmailVerified;
                    await _userManager.UpdateAsync(user);
                }
            }

            // Create new user if doesn't exist
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = googleUser.Email,
                    Email = googleUser.Email,
                    EmailConfirmed = googleUser.EmailVerified,
                    FirstName = googleUser.FirstName,
                    LastName = googleUser.LastName,
                    AvatarUrl = googleUser.Picture,
                    GoogleId = googleUser.GoogleId,
                    IsGoogleUser = true,
                    IsVerified = googleUser.EmailVerified,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                {
                    return ApiResponse<AuthResponseDto>.FailureResponse(
                        "Google authentication failed",
                        result.Errors.Select(e => e.Description).ToList());
                }

                // Assign default "User" role
                await _userManager.AddToRoleAsync(user, "User");
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Generate tokens
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken(ipAddress);

            // Save refresh token
            refreshToken.UserId = user.Id;
            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles.ToList();

            var authResponse = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = userDto
            };

            return ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "Google login successful");
        }

        public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(token);

            if (refreshToken == null || !refreshToken.IsActive)
            {
                return ApiResponse<AuthResponseDto>.FailureResponse(
                    "Invalid token",
                    new List<string> { "Token is invalid or expired" });
            }

            var user = refreshToken.User;

            if (user == null || !user.IsActive)
            {
                return ApiResponse<AuthResponseDto>.FailureResponse(
                    "User not found",
                    new List<string> { "User associated with token not found" });
            }

            // Revoke old token
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;

            // Generate new tokens
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken(ipAddress);

            // Link tokens
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            newRefreshToken.UserId = user.Id;

            // Save new refresh token
            await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
            await _unitOfWork.SaveChangesAsync();

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles.ToList();

            var authResponse = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = userDto
            };

            return ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "Token refreshed successfully");
        }

        public async Task<ApiResponse<bool>> RevokeTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(token);

            if (refreshToken == null || !refreshToken.IsActive)
            {
                return ApiResponse<bool>.FailureResponse(
                    "Invalid token",
                    new List<string> { "Token not found or already revoked" });
            }

            // Revoke token
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Token revoked successfully");
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return ApiResponse<bool>.FailureResponse(
                    "User not found",
                    new List<string> { "User not found" });
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                return ApiResponse<bool>.FailureResponse(
                    "Password change failed",
                    result.Errors.Select(e => e.Description).ToList());
            }

            // Revoke all refresh tokens (force re-login on all devices)
            await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Password changed successfully");
        }

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return ApiResponse<UserDto>.FailureResponse(
                    "User not found",
                    new List<string> { "User not found" });
            }

            var userDto = _mapper.Map<UserDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            userDto.Roles = roles.ToList();

            return ApiResponse<UserDto>.SuccessResponse(userDto);
        }

        public async Task<ApiResponse<UserDto>> GetCurrentUserAsync(string userId)
        {
            return await GetUserByIdAsync(userId);
        }
    }
}
