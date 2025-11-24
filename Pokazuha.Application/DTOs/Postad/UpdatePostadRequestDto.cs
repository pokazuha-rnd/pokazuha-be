using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokazuha.Application.DTOs.Postad
{
    public class UpdatePostadRequestDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public string Category { get; set; }
        public string Condition { get; set; }
        public string Location { get; set; }
        public string PhoneNumber { get; set; }
        public bool ShowEmailToPublic { get; set; }
        public List<IFormFile> NewImages { get; set; } = new List<IFormFile>();
        public List<Guid> ImageIdsToDelete { get; set; } = new List<Guid>();
    }
}
