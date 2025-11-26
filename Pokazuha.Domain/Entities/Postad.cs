using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokazuha.Domain.Entities
{
    public class Postad
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(5000)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "USD";

        [Required]
        [MaxLength(100)]
        public string Category { get; set; }

        [Required]
        [MaxLength(20)]
        public string Condition { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        public bool ShowEmailToPublic { get; set; } = false;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public int ViewCount { get; set; } = 0;

        public bool IsFeatured { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; } // ✨ Sau string dacă vrei să fie consistent

        public virtual ICollection<PostadImage> Images { get; set; } = new List<PostadImage>();
    }
}
