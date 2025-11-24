using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokazuha.Application.DTOs.Postad
{
    public class CreatePostadRequestDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "Lei";
        public string Category { get; set; }
        public string Condition { get; set; }
        public string Location { get; set; }
        public string PhoneNumber { get; set; }
        public bool ShowEmailToPublic { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}
