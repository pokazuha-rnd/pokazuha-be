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
    public class PostadConfiguration : IEntityTypeConfiguration<Postad>
    {
        public void Configure(EntityTypeBuilder<Postad> builder)
        {
            builder.ToTable("Postads");

            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.UserId)
                .IsRequired()
                .HasMaxLength(450); // ✨ ADAUGĂ pentru string FK

            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(5000);

            builder.Property(p => p.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("USD");

            builder.Property(p => p.Category)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Condition)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(p => p.Location)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(p => p.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            builder.Property(p => p.ViewCount)
                .HasDefaultValue(0);

            builder.Property(p => p.IsFeatured)
                .HasDefaultValue(false);

            builder.Property(p => p.ShowEmailToPublic)
                .HasDefaultValue(false);

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(p => p.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(p => p.UserId)
                .HasDatabaseName("IX_Postads_UserId");

            builder.HasIndex(p => p.Category)
                .HasDatabaseName("IX_Postads_Category");

            builder.HasIndex(p => p.Status)
                .HasDatabaseName("IX_Postads_Status");

            builder.HasIndex(p => p.CreatedAt)
                .HasDatabaseName("IX_Postads_CreatedAt");

            builder.HasIndex(p => p.Price)
                .HasDatabaseName("IX_Postads_Price");

            builder.HasIndex(p => p.Location)
                .HasDatabaseName("IX_Postads_Location");

            // Relationships
            builder.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Images)
                .WithOne(i => i.Postad)
                .HasForeignKey(i => i.PostadId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
