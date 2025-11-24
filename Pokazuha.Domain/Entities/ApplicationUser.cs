using Microsoft.AspNetCore.Identity;
namespace Pokazuha.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Location { get; set; }
        public string? Bio { get; set; }

        public string? GoogleId { get; set; }
        public bool IsGoogleUser { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        public string FullName => string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
            ? Email ?? UserName ?? "Unknown"
            : $"{FirstName} {LastName}".Trim();
    }
}
