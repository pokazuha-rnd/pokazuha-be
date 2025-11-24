using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokazuha.Application.DTOs.Postad
{
    public class PostadListDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public string Category { get; set; }
        public string Condition { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PrimaryImageUrl { get; set; }
        public string UserName { get; set; }
    }
}
