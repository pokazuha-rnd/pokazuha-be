using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokazuha.Domain.Entities
{
    public class PostadImage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PostadId { get; set; }

        [ForeignKey(nameof(PostadId))]
        public Postad Postad { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; }

        public bool IsPrimary { get; set; } = false;

        public int Order { get; set; } = 0;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string FileName { get; set; }

        public long FileSize { get; set; } // in bytes
    }
}
