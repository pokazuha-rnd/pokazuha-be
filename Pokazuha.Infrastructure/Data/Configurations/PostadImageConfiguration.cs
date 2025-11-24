using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pokazuha.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokazuha.Infrastructure.Data.Configurations
{
    public class PostadImageConfiguration : IEntityTypeConfiguration<PostadImage>
    {
        public void Configure(EntityTypeBuilder<PostadImage> builder)
        {
            builder.ToTable("PostadImages");

            // Primary Key
            builder.HasKey(i => i.Id);

            // Properties
            builder.Property(i => i.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(i => i.FileName)
                .HasMaxLength(100);

            builder.Property(i => i.IsPrimary)
                .HasDefaultValue(false);

            builder.Property(i => i.Order)
                .HasDefaultValue(0);

            builder.Property(i => i.UploadedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(i => i.FileSize)
                .IsRequired();

            // Indexes
            builder.HasIndex(i => i.PostadId)
                .HasDatabaseName("IX_PostadImages_PostadId");

            builder.HasIndex(i => new { i.PostadId, i.IsPrimary })
                .HasDatabaseName("IX_PostadImages_PostadId_IsPrimary");

            // Relationship (already defined in PostadConfiguration, but explicit here)
            builder.HasOne(i => i.Postad)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.PostadId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
