using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pokazuha.Domain.Entities;

namespace Pokazuha.Infrastructure.Data.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Table Name (already set in DbContext, but can override here)
            builder.ToTable("Users");

            // Properties
            builder.Property(u => u.FirstName)
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .HasMaxLength(100);

            builder.Property(u => u.Location)
                .HasMaxLength(200);

            builder.Property(u => u.Bio)
                .HasMaxLength(1000);

            builder.Property(u => u.AvatarUrl)
                .HasMaxLength(500);

            builder.Property(u => u.GoogleId)
                .HasMaxLength(100);

            builder.Property(u => u.IsActive)
                .HasDefaultValue(true);

            builder.Property(u => u.IsVerified)
                .HasDefaultValue(false);

            builder.Property(u => u.IsGoogleUser)
                .HasDefaultValue(false);

            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(u => u.Email)
                .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(u => u.GoogleId)
                .HasDatabaseName("IX_Users_GoogleId")
                .IsUnique()
                .HasFilter("[GoogleId] IS NOT NULL");

            builder.HasIndex(u => u.CreatedAt)
                .HasDatabaseName("IX_Users_CreatedAt");

            // Relationships
            builder.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore computed property
            builder.Ignore(u => u.FullName);
        }
    }
}
